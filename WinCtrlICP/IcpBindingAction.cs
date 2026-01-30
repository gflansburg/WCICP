using System;
using System.Collections.Generic;
using System.Text;

namespace WinCtrlICP
{
    public enum IcpBindingActionKind
    {
        ShowBuiltIn,
        ShowCustom,
        CycleAllNext,
        CycleAllPrev,
        CycleCustomNext,
        CycleCustomPrev
    }

    public sealed class IcpBindingAction
    {
        public IcpBindingActionKind Kind { get; set; }

        // For ShowBuiltIn
        public Page? BuiltInPage { get; set; }

        // For ShowCustom
        public Guid? CustomDisplayId { get; set; }

        public override string ToString()
        {
            return Kind switch
            {
                IcpBindingActionKind.ShowBuiltIn => $"Show {BuiltInPage}",
                IcpBindingActionKind.ShowCustom => $"Show Custom ({CustomDisplayId})",
                IcpBindingActionKind.CycleAllNext => "Next Display (All)",
                IcpBindingActionKind.CycleAllPrev => "Previous Display (All)",
                IcpBindingActionKind.CycleCustomNext => "Next Display (Custom)",
                IcpBindingActionKind.CycleCustomPrev => "Previous Display (Custom)",
                _ => Kind.ToString()
            };
        }
    }
}
