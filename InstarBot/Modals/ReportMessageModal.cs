using Discord;
using Discord.Interactions;

namespace PaxAndromeda.Instar.Modals;

public class ReportMessageModal : IModal
{
    public string Title => "Report Message";

    [InputLabel("Why are you reporting this message?")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, placeholder: "Reason for reporting", minLength: 12)]
    public string ReportReason { get; set; }
}