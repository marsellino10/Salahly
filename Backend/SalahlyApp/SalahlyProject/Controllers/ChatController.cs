using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SalahlyProject.Contracts.Chat;
using SalahlyProject.Response;
using SalahlyProject.Services.Chat;


namespace SalahlyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Ask a question to the chatbot
        /// </summary>
        [HttpPost("ask")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ChatResponseDto>>> AskAsync(
            [FromBody] ChatRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ChatResponseDto>(400, "Invalid request payload."));
            }

            try
            {
                var question = request.Question.Trim();
                var (answer, isFallback) = await _chatService.AskAsync(question, request.Context, cancellationToken);

                var response = new ChatResponseDto
                {
                    Answer = answer,
                    IsFallback = isFallback,
                };

                return Ok(new ApiResponse<ChatResponseDto>(200, "Response generated successfully.", response));
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest, 
                    new ApiResponse<ChatResponseDto>(499, "Request cancelled by client."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<ChatResponseDto>(500, "Unable to process the request at the moment."));
            }
        }
    }
}
