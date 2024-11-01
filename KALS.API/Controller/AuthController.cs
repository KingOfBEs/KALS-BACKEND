using KALS.API.Constant;
using KALS.API.Models.User;
using KALS.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace KALS.API.Controller;
////////////////////////////////////////////////////////////////////
//                          _ooOoo_                               //
//                         o8888888o                              //
//                         88" . "88                              //
//                         (| ^_^ |)                              //
//                         O\  =  /O                              //
//                      ____/`---'\____                           //
//                    .'  \\|     |//  `.                         //
//                   /  \\|||  :  |||//  \                        //
//                  /  _||||| -:- |||||-  \                       //
//                  |   | \\\  -  /// |   |                       //
//                  | \_|  ''\---/''  |   |                       //
//                  \  .-\__  `-`  ___/-. /                       //
//                ___`. .'  /--.--\  `. . ___                     //
//              ."" '<  `.___\_<|>_/___.'  >'"".                  //
//            | | :  `- \`.;`\ _ /`;.`/ - ` : | |                 //
//            \  \ `-.   \_ __\ /__ _/   .-` /  /                 //
//      ========`-.____`-.___\_____/___.-`____.-'========         //
//                           `=---='                              //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^        //
//                                                                //
////////////////////////////////////////////////////////////////////
[ApiController]
[Route(ApiEndPointConstant.ApiEndpoint)]
public class AuthController: BaseController<AuthController>
{
    private readonly IUserService _userService;
    public AuthController(ILogger<AuthController> logger, IUserService userService) : base(logger)
    {
        _userService = userService;
    }
    
    [HttpPost(ApiEndPointConstant.Auth.SendOtp)]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendOtp([FromBody] GenerateOtpRequest request)
    {
        var result = await _userService.GenerateOtpAsync(request);
        if (result == null)
        {
            return Problem(MessageConstant.Sms.SendSmsFailed);
        }

        return CreatedAtAction(nameof(SendOtp), result);
    }
    [HttpPost(ApiEndPointConstant.Auth.Signup)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Signup([FromBody] RegisterRequest registerRequest)
    {
        var loginResponse = await _userService.RegisterAsync(registerRequest);
        if (loginResponse == null)
        {
            _logger.LogError($"Sign up failed with {registerRequest.Username}");
            return Problem(MessageConstant.User.RegisterFail);
        }
        _logger.LogInformation($"Sign up successful with {registerRequest.Username}");
        return CreatedAtAction(nameof(Signup), loginResponse);
    }
    [HttpPost(ApiEndPointConstant.Auth.Login)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var loginResponse = await _userService.LoginAsync(loginRequest);
        if (loginResponse == null)
        {
            _logger.LogError($"Login failed with {loginRequest.UsernameOrPhoneNumber}");
            return Problem(MessageConstant.User.LoginFail);
        }
        _logger.LogInformation($"Login successful with {loginRequest.UsernameOrPhoneNumber}");
        return Ok(loginResponse);
    }
    [HttpPatch(ApiEndPointConstant.Auth.ForgetPassword)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest forgetPasswordRequest)
    {
        var userResponse = await _userService.ForgetPassword(forgetPasswordRequest);
        if (userResponse == null)
        {
            _logger.LogError($"Forget password failed with {forgetPasswordRequest.PhoneNumber}");
            return Problem(MessageConstant.User.ForgetPasswordFail);
        }
        _logger.LogInformation($"Forget password successful with {forgetPasswordRequest.PhoneNumber}");
        return Ok(userResponse);
    }

}