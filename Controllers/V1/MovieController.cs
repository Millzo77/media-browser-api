using Microsoft.AspNetCore.Mvc;
// Controllers/MovieController.cs
[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase {
    private readonly MovieService _movieService;

    public MovieController(MovieService movieService) {
        _movieService = movieService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query) {
        var movies = await _movieService.SearchMoviesAsync(query);
        return Ok(movies);
    }
}