namespace KALS.API.Constant;

public class ApiEndPointConstant
{
    static ApiEndPointConstant() {}
    
    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;
    
    public class User
    {
        public const string UserEndpoint = ApiEndpoint + "/user";
        public const string Login = UserEndpoint + "/login";
    }

    public class Product
    {
        public const string ProductEndpoint = ApiEndpoint + "/products";
        public const string ProductById = ProductEndpoint + "/{id}";
        public const string UpdateProductRelationship = ProductById + "/product-relationship";
        public const string LabToProduct = ProductById + "/lab";
        public const string CartByProductId = ProductById + "/cart";
        public const string ProductImage = ProductById + "/product-image";
    }

    public class Lab
    {
        public const string LabEndPoint = ApiEndpoint + "/labs";
        public const string LabById = LabEndPoint + "/{id}";
    }

    public class Category
    {
        public const string CategoryEndPoint = ApiEndpoint + "/categories";
        public const string CategoryById = CategoryEndPoint + "/{id}";
        public const string UpdateProductCategory = CategoryById + "/product-category";
        public const string ProductByCategoryId = CategoryById + "/product";
    }
    public class Cart
    {
        public const string CartEndPoint = ApiEndpoint + "/carts";
    }

    public class Auth
    {
        public const string SendOtp = ApiEndpoint + "/otp";
        public const string Signup = ApiEndpoint + "/signup";
        public const string Login = ApiEndpoint + "/login";
        public const string ForgetPassword = ApiEndpoint + "/forget-password";
    }
    public class Payment
    {
        public const string PaymentEndPoint = ApiEndpoint + "/payments";
        public const string PaymentCheckOut = PaymentEndPoint + "/checkout";
    }

    public class Staff
    {
        public const string StaffEndpoint = ApiEndpoint + "/staffs";
        public const string StaffById = StaffEndpoint + "/{id}";
    }

    public class Member
    {
        public const string MemberEndpoint = ApiEndpoint + "/members";
        public const string MemberById = MemberEndpoint + "/{id}";
        public const string MemberInformation = MemberEndpoint + "/information";
    }
    public class Order
    {
        public const string OrderEndpoint = ApiEndpoint + "/orders";
        public const string OrderById = OrderEndpoint + "/{id}";
        public const string UpdateOrderStatus = OrderById + "/status";
        public const string OrderItems = OrderById + "/order-items";
    }

    public class SupportRequest
    {
        public const string SupportRequestEndPoint = ApiEndpoint + "/support-requests";
        public const string SupportRequestById = SupportRequestEndPoint + "/{id}";
        public const string SupportMessage = SupportRequestById + "/messages";
        
    }

    public class ProductImage
    {
        public const string ProductImageEndPoint = ApiEndpoint + "/product-images";
        public const string ProductImageById = ProductImageEndPoint + "/{id}";
    }
}