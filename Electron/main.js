const {
  app,
  BrowserWindow,
} = require('electron')
const electron = require('electron')
const net = require("net");
const path = require("path");
const fs = require("fs");

const WWTHID = require('./mainsrc/WWTHID')
const F16ICP = require('./mainsrc/F16_ICP.js');

const PIPE_NAME = "WCICP_F16DED";
const PIPE_PATH = "\\\\.\\pipe\\" + PIPE_NAME;

const HEADLESS = process.argv.includes("--headless") || process.env.WCICP_HEADLESS === "1";
const LOGGING = process.argv.includes("--logging") || process.env.WCICP_LOGGING === "1";

let pipeServer = null;
let pipeSockets = new Set();

function updateClients() {
  uiSetText("clients", String(pipeSockets.size));
}

/* ---------- logging ---------- */

function logFile(msg) {
  try {
	if (LOGGING) {
      fs.appendFileSync(
        path.join(__dirname, "bridge.log"),
        new Date().toISOString() + " " + msg + "\r\n"
      );
	}
  } catch (_) {}
}

// Send status to window
function uiSend(channel, payload) {
  try {
    if (mainWindow && !mainWindow.isDestroyed() && mainWindow.webContents) {
      mainWindow.webContents.send(channel, payload);
    }
  } catch (_) {}
}

function safeJson(obj) {
  try { return JSON.stringify(obj); }
  catch (_) { return "{\"ok\":false,\"error\":\"JSON stringify failed\"}"; }
}

function sendLine(socket, obj) {
  const s = safeJson(obj);
  logFile("TX " + s);
  try { socket.write(s + "\n"); } catch (_) {}
}

/* ---------- ICP helpers ---------- */

function normalizeLines(lines) {
  const out = ["", "", "", "", ""];
  if (!Array.isArray(lines)) return out;
  for (let i = 0; i < Math.min(5, lines.length); i++) {
    out[i] = (lines[i] === undefined || lines[i] === null) ? "" : String(lines[i]);
  }
  return out;
}

// --- ICP text rendering (pixel-coordinate) ---
//
// Markers (do NOT consume a character cell):
//   ⟦  start inverted run
//   ⟧  end inverted run
//
// Example:
//   "UHF ⟦305.000⟧ VHF 122.800"
//

const INV_START = "⟦";
const INV_END   = "⟧";

const ICP_GLYPH_W = 8;
const ICP_GLYPH_H = 13;

const MAX_ROWS = 5;
const MAX_COLS = 25;

const GLYPH_MAP = {
  '↕': 'a',
  '↑': 'u',
  '↓': 'd',
  '°': 'o'
};

function mapGlyph(ch) {
  return GLYPH_MAP[ch] ?? ch.toUpperCase();
}

function drawIcp(lines, font) {
  font = font || "DCS";
  lines = Array.isArray(lines) ? lines : [];

  const api = WWTHID.WWTHID_JSAPI;

  api.ICPDrawStart();

  for (let row = 0; row < Math.min(lines.length, MAX_ROWS); row++) {
    const text = lines[row] == null ? "" : String(lines[row]);

    let inv = false;
    let cell = 0;

    for (let i = 0; i < text.length; i++) {
      const ch = text[i];

      if (ch === INV_START) { inv = true; continue; }
      if (ch === INV_END)   { inv = false; continue; }

      if (cell >= MAX_COLS) break;

      const x = cell * ICP_GLYPH_W;
      const y = row  * ICP_GLYPH_H;

	  const outChar = mapGlyph(ch);
      api.ICPDraw(x, y, outChar, inv, font);
      cell++;
    }
  }

  api.ICPDrawEnd();

  uiSend("icp:update", {
    lines,
    font,
    glyphW: ICP_GLYPH_W,
    glyphH: ICP_GLYPH_H
  });
}

function clearIcp() {
  drawIcp(["", "", "", "", ""], "DCS");
}

let mainWindow;

function createWindow () {
  mainWindow = new BrowserWindow({
    minWidth: 400,
    minHeight: 400,
    width: 400,
    height: 400,
    backgroundColor: '#ffffff',
    icon: path.join(__dirname, 'www/logo', 'SimAppPro.png'),
    webPreferences: {
      webSecurity: false,
      nodeIntegration: true
    },
    show: !HEADLESS,
	skipTaskbar: HEADLESS,
    focusable: !HEADLESS
  })

  if (HEADLESS) mainWindow.hide();

// Simple built-in UI so you don’t need index.html
  const html = `
<!doctype html>
<html>
<head>
<meta charset="utf-8"/>
<title>F16DEDWriter Bridge</title>
<style>
  body { font-family: Segoe UI, Arial, sans-serif; margin: 12px; }
  .row { display: flex; gap: 12px; }
  .card { border: 1px solid #ccc; border-radius: 8px; padding: 10px; flex: 1; }
  .mono { font-family: Consolas, monospace; white-space: pre; }
  .small { font-size: 12px; color: #444; }
  .lines { margin-top: 8px; padding: 8px; background: #f7f7f7; border-radius: 6px; }
  .log { height: 210px; overflow: auto; background: #111; color: #ddd; padding: 8px; border-radius: 6px; }
  .badge { display:inline-block; padding:2px 8px; border-radius: 999px; background:#eee; }
</style>
</head>
<body>
  <h2>F16DEDWriter Bridge <span id="status" class="badge">starting</span></h2>

  <div class="row">
    <div class="card">
      <div><b>Pipe</b></div>
      <div class="mono small" id="pipe"></div>
      <div class="small">PID: <span class="mono" id="pid"></span></div>
      <div class="small">Clients: <span class="mono" id="clients">0</span></div>
    </div>
    <div class="card">
      <div><b>Last Command</b></div>
      <div class="mono small" id="lastCmd">(none)</div>
    </div>
  </div>

  <div class="card" style="margin-top:12px;">
    <div><b>Last ICP Write</b></div>
    <div class="lines mono" id="lines"></div>
  </div>

  <div class="card" style="margin-top:12px;">
    <div><b>Bridge Log</b></div>
    <div class="log mono" id="log"></div>
  </div>
</body>
</html>`;

  mainWindow.loadURL("data:text/html;charset=utf-8," + encodeURIComponent(html));

  var OpenDevicesState = false;

  function InitCallBack () {
    WWTHID.WWTHID_JSAPI.CB_Data(function (agr) {
      mainWindow.webContents.send('WWTHID_CB_Data', agr);
    });
    WWTHID.WWTHID_JSAPI.CB_InputData(function (agr) {
      mainWindow.webContents.send('WWTHID_CB_InputData', agr);
    });
    WWTHID.WWTHID_JSAPI.CB_UpdateProgress(function (agr) {
      mainWindow.webContents.send('WWTHID_CB_UpdateProgress', agr);
    });
    WWTHID.WWTHID_JSAPI.CB_Read(function (agr) {
      mainWindow.webContents.send('WWTHID_CB_Read', agr);
    });
  }

  var addCB_PartChange = function () {
    if (OpenDevicesState) {
      WWTHID.WWTHID_JSAPI.CB_PartChange(function (agr) {
      });
    }
  }

  var addCB_DeviceChange = function () {
    if (OpenDevicesState) {
      WWTHID.WWTHID_JSAPI.CB_DeviceChange(function (agr) {
      });
    }
  }

  var OpenDevicesFunc = function (event, arg) {
    var mainPath = path.join(__dirname.replace('app.asar', 'app.asar.unpacked'));
    WWTHID.WWTHID_JSAPI.OpenDevices(mainPath);
    InitCallBack();

    OpenDevicesState = true;

    addCB_PartChange();
    addCB_DeviceChange();

    if (event !== undefined) {
      event.returnValue = true;
    }
  };

  var CloseDevicesFunc = function (event, arg) {
    WWTHID.WWTHID_JSAPI.CloseDevices();

    OpenDevicesState = false;
    if (event !== undefined) {
      event.returnValue = true;
    }
  };

  mainWindow.on('closed', function () {
    WWTHID.WWTHID_JSAPI.CB_Data(function (agr) { });
    WWTHID.WWTHID_JSAPI.CB_InputData(function (agr) { });
    WWTHID.WWTHID_JSAPI.CB_UpdateProgress(function (agr) { });
    WWTHID.WWTHID_JSAPI.CB_PartChange(function (agr) { });
    WWTHID.WWTHID_JSAPI.CB_DeviceChange(function (agr) { });
    mainWindow = null;
    app.quit();
  })

  if (!HEADLESS) {
    mainWindow.once("ready-to-show", () => {
      mainWindow.show();
    });
  }

  mainWindow.on("closed", () => {
    mainWindow = null;
  });
  
  OpenDevicesFunc()
}

/* ---------- Minimal UI injection helpers (no preload needed) ---------- */

function uiSetText(id, text) {
  if (!mainWindow || mainWindow.isDestroyed()) return;
  const safe = JSON.stringify(String(text));
  mainWindow.webContents.executeJavaScript(
    `(() => { const el = document.getElementById(${JSON.stringify(id)}); if (el) el.textContent = ${safe}; })();`,
    true
  ).catch(() => {});
}

function uiAppendLog(text) {
  if (!mainWindow || mainWindow.isDestroyed()) return;
  const safe = JSON.stringify(String(text));
  mainWindow.webContents.executeJavaScript(
    `(() => {
      const el = document.getElementById("log");
      if (!el) return;
      el.textContent += ${safe} + "\\n";
      el.scrollTop = el.scrollHeight;
    })();`,
    true
  ).catch(() => {});
}

function uiSetLines(lines) {
  if (!mainWindow || mainWindow.isDestroyed()) return;
  const safe = JSON.stringify(lines.join("\n"));
  mainWindow.webContents.executeJavaScript(
    `(() => { const el = document.getElementById("lines"); if (el) el.textContent = ${safe}; })();`,
    true
  ).catch(() => {});
}

/* ---------- Pipe server ---------- */

function startPipeServer() {
  pipeServer = net.createServer();
  
  function updateClients() {
    uiSetText("clients", String(pipeSockets.size));
  }

  pipeServer.on("connection", (socket) => {
    pipeSockets.add(socket);
    socket.setEncoding("utf8");
    logFile("CLIENT CONNECT");
    uiAppendLog("CLIENT CONNECT");
    updateClients();

    sendLine(socket, { type: "ready", pipe: PIPE_NAME, pid: process.pid });

    let buffer = "";

    socket.on("data", (chunk) => {
      buffer += chunk;

      let idx;
      while ((idx = buffer.indexOf("\n")) >= 0) {
        const line = buffer.slice(0, idx).trim();
        buffer = buffer.slice(idx + 1);
        if (!line) continue;

        logFile("RX " + line);
        uiAppendLog("RX " + line);
        uiSetText("lastCmd", line);

        let msg;
        try { msg = JSON.parse(line); }
        catch (_) {
          sendLine(socket, { id: null, ok: false, error: "Invalid JSON" });
          continue;
        }

        const id = (msg.id === undefined || msg.id === null) ? null : msg.id;
        const cmd = msg.cmd;
        const arg = msg.arg || {};

        try {
          if (cmd === "ping") {
            sendLine(socket, { id, ok: true, result: "pong" });
            continue;
          }

          if (cmd === "drawIcp") {
			drawIcp(arg.lines, arg.font);
            uiSetLines(normalizeLines(arg.lines));
            sendLine(socket, { id, ok: true, result: true });
            continue;
          }

          if (cmd === "clearIcp") {
            clearIcp();
            sendLine(socket, { id, ok: true, result: true });
            continue;
          }

		  if (cmd === "close") {
		    sendLine(socket, { id, ok: true, result: true });
            uiAppendLog("CLOSE: shutting down");
		    safeClearAndQuit();
		    return;
		  }

          sendLine(socket, { id, ok: false, error: "Unknown cmd: " + cmd });
        } catch (e) {
          sendLine(socket, { id, ok: false, error: e ? e.message : "Command failed" });
        }
      }
    });

    socket.on("close", () => {
      pipeSockets.delete(socket);
      logFile("CLIENT DISCONNECT");
      uiAppendLog("CLIENT DISCONNECT");
      updateClients();
    });

    socket.on("error", (err) => {
      logFile("SOCKET ERROR: " + (err ? err.message : "unknown"));
      uiAppendLog("SOCKET ERROR: " + (err ? err.message : "unknown"));
    });
  });

  pipeServer.on("error", (err) => {
    const msg = err ? (err.code + " " + err.message) : "unknown";
    logFile("SERVER ERROR: " + msg);
    uiAppendLog("SERVER ERROR: " + msg);

    if (err && err.code === "EADDRINUSE") {
      uiSetText("status", "pipe in use");
      app.exit(10);
      return;
    }
    app.exit(11);
  });

  pipeServer.listen(PIPE_PATH, () => {
    logFile("PIPE LISTEN " + PIPE_PATH);
    uiAppendLog("PIPE LISTEN " + PIPE_PATH);
    uiSetText("status", "ready");
    uiSetText("pipe", PIPE_PATH);
    uiSetText("pid", String(process.pid));
  });
}

app.on("ready", function () {
  createWindow();
  startPipeServer();
});

let isQuitting = false;

function safeClearAndQuit() {
  if (isQuitting) return;
  isQuitting = true;

  try { clearIcp(); } catch (_) {}

  try { WWTHID.WWTHID_JSAPI.CloseDevices(); } catch (_) {}

  // Close pipe clients
  for (const s of pipeSockets) {
    try { s.end(); } catch (_) {}
  }
  pipeSockets.clear();
  updateClients();

  // Close pipe server, then quit
  if (pipeServer) {
    try {
      pipeServer.close(() => setTimeout(() => {
        try { app.quit(); } catch (_) { process.exit(0); }
      }, 150));
    } catch (_) {
      setTimeout(() => { try { app.quit(); } catch (_) { process.exit(0); } }, 150);
    }
  } else {
    setTimeout(() => { try { app.quit(); } catch (_) { process.exit(0); } }, 150);
  }
}

app.on("before-quit", (event) => {
  // Take control of shutdown so clearIcp actually reaches the device
  if (!isQuitting) {
    event.preventDefault();
    safeClearAndQuit();
  }
});

app.on("window-all-closed", () => {
  // UI semantics only — delegate to our controlled quit
  if (process.platform !== "darwin") safeClearAndQuit();
});

app.on('web-contents-created', function () {
  console.log('web-contents-created');
})

app.on('activate', function () {
  if (mainWindow === null) {
    createWindow();
  }
})

