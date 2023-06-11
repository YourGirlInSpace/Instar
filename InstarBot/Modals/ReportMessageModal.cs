using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace PaxAndromeda.Instar.Modals;

[UsedImplicitly]
public class ReportMessageModal : IModal
{
    [InputLabel("Why are you reporting this message?")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, "Reason for reporting", 12)]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string? ReportReason { get; set; }

    [ExcludeFromCodeCoverage] public string Title => "Report Message";
}