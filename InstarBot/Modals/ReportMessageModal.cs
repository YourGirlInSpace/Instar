using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;

namespace PaxAndromeda.Instar.Modals;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class ReportMessageModal : IModal
{
    [InputLabel("Why are you reporting this message?")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, "Reason for reporting", 12)]
    public string? ReportReason { get; set; }

    public string Title => "Report Message";
}