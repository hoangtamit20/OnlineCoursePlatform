namespace OnlineCoursePlatform.Helpers.Emails.RenderHtmls
{
    public static class RenderHtmlHelper
    {
        public static string GetHtmlConfirmEmail(Uri urlConfirm, string email) => $@"<table width='100%' border='0' cellspacing='0' cellpadding='0'>
        <tr>
            <td align='center'>
                <div style='width: 698px; height: 560.76px; position: relative'>
                    <img alt='' style='width: 221.96px; height: 180px; display: block; margin: auto;'
                        src='https://i.vimeocdn.com/portrait/95014017_640x640'>
                    <div
                        style='margin-top: 50px; margin-bottom: 50px; color: #EF02D7; font-size: 25px; font-family: Inter; font-weight: 700; word-wrap: break-word; text-align: center;'>
                        VEdu Online Course Platform</div>
                    <div
                        style='margin-bottom: 50px; width: 698px; height: 97px; text-align: justify; color: #857B7B; font-size: 25px; font-family: Inria Serif; font-style: italic; font-weight: 300; word-wrap: break-word'>
                        We've sent an email to {email} to verify your email address and activate your
                        account. The
                        link
                        in the email will expire in 24 hours.
                        <br />
                    </div>
                    <div
                        style='display: table; width: 387.61px; height: 51.26px; background: linear-gradient(90deg, #4776e6 0%, #8e54e9 100%); border-radius: 15px; margin: auto;'>
                        <div style='display: table-cell; vertical-align: middle; text-align: center;'>
                            <a href='{urlConfirm}'
                                style='text-decoration: none; color: white; font-size: 24px; font-family: Work Sans; font-weight: 600; word-wrap: break-word;'>
                                Verify your Email<br></a>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
    </table>";
    }
}