namespace KALS.API.Constant;

public class MessageConstant
{
    public static class User
    {
        public const string UserNotFound = "Username không tồn tại";
        public const string PasswordIncorrect = "Mật khẩu không chính xác";
        public const string PhoneNumberNotFound = "Số điện thoại không tồn tại";
        public const string RegisterFail = "Đăng ký không thành công";
        public const string UserNameExisted = "Tên đăng nhập đã tồn tại";
        public const string PhoneNumberExisted = "Số điện thoại đã tồn tại";
        public const string LoginFail = "Đăng nhập không thành công";
        public const string MemberAddressNotFound = "Địa chỉ thành viên không tồn tại";
        public const string ForgetPasswordFail = "Quên mật khẩu không thành công";
        public const string UserIdNotNull = "Id user không được để trống";
        public const string MemberNotFound = "Thành viên không tồn tại";
        public const string UpdateMemberFail = "Cập nhật thành viên không thành công";
        public const string RoleNotFound = "Role không tồn tại";
        public const string StaffNotFound = "Staff không tồn tại";
        public const string UpdateStaffFail = "Cập nhật staff không thành công";
    }

    public static class Order
    {
        public const string CreateOrderFail = "Tạo đơn hàng không thành công";
        public const string OrderIdNotNull = "Id đơn hàng không được để trống";
        public const string OrderNotFound = "Đơn hàng không tồn tại";
        public const string OrderStatusNotFound = "Trạng thái đơn hàng không tồn tại";
        public const string UpdateOrderStatusFail = "Cập nhật trạng thái đơn hàng không thành công";
    }
    public static class Product
    {
        public const string ChildProductNotFound = "Sản phẩm con không tồn tại";
        public const string CreateProductFail = "Tạo sản phẩm không thành công với tên";
        public const string ProductIdNotNull = "Id sản phẩm không được để trống";
        public const string ProductNotFound = "Sản phẩm không tồn tại";
        public const string UpdateProductFail = "Cập nhật sản phẩm không thành công";
        public const string UpdateProductRelationshipFail = "Cập nhật quan hệ sản phẩm không thành công";
        public const string ParentProductIdNotNull = "Id của sản phẩm kit không được để trống";
        public const string ChildProductIdNotNull = "Id của sản phẩm con không được để trống";
        public const string ProductOutOfStock = "Sản phẩm đã hết hàng";
    }
    public static class Lab
    {
        public const string AssignLabToProductFail = "Gán lab cho sản phẩm không thành công";
        public const string LabIdNotNull = "Id lab không được để trống";
        public const string UploadFileFail = "Upload file không thành công";
        public const string CreateLabFail = "Tạo lab không thành công";
    }

    public static class Category
    {
        public const string  CategoryIdNotNull = "Id danh mục không được để trống";
        public const string CategoryNotFound = "Danh mục không tồn tại";
        public const string UpdateProductCategoryFail = "Cập nhật danh mục của sản phẩm không thành công";
        public const string UpdateCategoryFail = "Cập nhật danh mục không thành công";
        public const string CreateCategoryFail = "Tạo danh mục không thành công";
    }

    public static class Cart
    {
        public const string AddToCartFail = "Thêm sản phẩm vào giỏ hàng không thành công";
        public const string RemoveFromCartFail = "Xóa sản phẩm khỏi giỏ hàng không thành công";
        public const string QuantityMustBeGreaterThanZero = "Số lượng sản phẩm phải lớn hơn 0";
        public const string UpdateQuantityFail = "Cập nhật số lượng sản phẩm không thành công";
    }

    public static class Sms
    {
        public const string SendSmsFailed = "Gửi tin nhắn không thành công";
        public const string OtpAlreadySent = "Mã OTP đã được gửi";
        public const string OtpNotFound = "Mã OTP không tồn tại";
        public const string OtpIncorrect = "Mã OTP không chính xác";
    }

    public static class Payment
    {
        public const string YourOrderIsPaid = "Đơn hàng của bạn đã được thanh toán";
        public const string YourOrderIsCancelled = "Đơn hàng của bạn đã bị hủy";
        public const string YourOrderIsCompleted = "Đơn hàng của bạn đã hoàn thành";
        public const string CannotFindPaymentLinkInformation = "Không thể tìm thấy thông tin link thanh toán";
        public const string YourOrderIsNotPaid = "Đơn hàng chưa được thanh toán";
        public const string CannotUpdateStatusPaymentAndOrder = "Không thể cập nhật trạng thái thanh toán và đơn hàng";
        public const string UpdateStatusPaymentAndOrderFail = "Cập nhật trạng thái thanh toán và đơn hàng không thành công";
        public const string PaymentNotFound = "Thanh toán không tồn tại";
        public const string OrderCodeNotNull = "OrderCode không được để trống";
        public const string CreatePaymentFail = "Tạo thanh toán không thành công";
        public const string FailToCreatePaymentLink = "Tạo link thanh toán không thành công";
        public const string PayOsStatusNotTrue = "Trạng thái thanh toán của PayOs không hợp lệ";
    }

    public static class OrderItem
    {
        public const string CreateOrderItemFail = "Tạo order item không thành công";
    }
}