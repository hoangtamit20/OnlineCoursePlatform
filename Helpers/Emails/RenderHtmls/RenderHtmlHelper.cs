namespace OnlineCoursePlatform.Helpers.Emails.RenderHtmls
{
    public static class RenderHtmlHelper
    {
        public static string GetHtmlConfirmEmail(Uri urlConfirm) => $@"
        <div style='background-color: #F4F5FF; display: flex; justify-content: center; align-items: center; height: auto;'>
            <div style='background-color: #FAFBFF; width: 60%; height: auto; padding: 20px; display: flex; flex-direction: column; justify-content: space-between;'>
                <header style='display: flex; justify-content: space-between;'>
                    <img src='https://www.mybestwebsitebuilder.com/storage/media/images/hostinger-review-logo-big.o.png' alt='Logo' style='width: 150px; height: auto;'>
                    <p style='color: #808080;'>Ba.Hai.Online</p>
                </header>
                <main>
                    <h1 style='color: #2F1C6A;'>Xác nhận địa chỉ email của bạn</h1>
                    <p style='color: #808080;'>Nhấn vào nút bên dưới để xác nhận địa chỉ email của bạn</p>
                    <br>
                    <a href='{urlConfirm}' style='padding: 15px 30px; background-color: #2F1C6A; color: white; border: none; border-radius: 5px; font-size: 1.2em; text-decoration: none;'>Xác Nhận Mail</a>
                </main>
                <div style='height: 1px; background-color: #808080; margin-top: 50px;'></div>
                <footer style='text-align: left; margin-top: 0px;'>
                    <p style='color: #808080;'>Bạn nhận được email này bởi vì bạn đã đăng ký tài khoản tại VEdu, để đảm bảo tuân thủ Điều khoản dịch vụ hoặc các vấn đề pháp lý khác của chúng tôi.</p>
                    <a href='#' style='color: #808080;'>Chính sách bảo mật</a>
                    <p style='color: #808080;'>© 2024-2024 VEdu International Ltd.</p>
                </footer>
            </div>
        </div>";
    }
}