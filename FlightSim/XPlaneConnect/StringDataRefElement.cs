using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FlightSim.XPlaneConnect
{
    public class StringDataRefElement : DataRefElement
    {
        private static readonly object lockElement = new object();
        public int StringLength { get; set; }
        public DateTime LastUpdateTime { get; set; }
        new public string Value { get; set; } = string.Empty;
        new public List<NotifyChangeHandler> Delegates = new List<NotifyChangeHandler>();

        public List<NotifyChangeHandler> ValueChangedDelegates = new List<NotifyChangeHandler>();
        public List<NotifyChangeHandler> ValueCompleteDelegates = new List<NotifyChangeHandler>();

        new public delegate void NotifyChangeHandler(StringDataRefElement sender, string newValue);
        new public event NotifyChangeHandler? OnValueChange;

        public event NotifyChangeHandler? OnValueComplete;

        public int CharactersInitialized { get; set; }

        public bool IsCompletelyInitialized
        {
            get
            {
                return CharactersInitialized >= StringLength;
            }
        }

        protected override void ValueChanged()
        {
            OnValueChange?.Invoke(this, Value);
        }

        protected void ValueComplete()
        {
            OnValueComplete?.Invoke(this, Value);
        }

        public void Update(int index, char character)
        {
            lock (lockElement)
            {
                LastUpdateTime = DateTime.Now;

                if (!IsCompletelyInitialized)
                    CharactersInitialized++;

                string value = Value;

                if (character > 0)
                {
                    if (Value.Length <= index)
                        Value = Value.PadRight(index + 1, ' ');

                    var current = Value[index];
                    if (current != character)
                    {
                        Value = Value.Remove(index, 1).Insert(index, character.ToString());
                    }
                }

                var fireEvent = !value.Equals(Value) || value.Length != Value.Length;

                if (fireEvent || ForceUpdate)
                {
                    ValueChanged();
                    //CharactersInitialized = 0;
                    if (IsCompletelyInitialized)
                    {
                        ForceUpdate = false;
                        ValueComplete();
                    }
                }
            }
        }

        public StringDataRefElement()
        {
            CharactersInitialized = 0;
            Value = string.Empty;
        }
    }
}
