public class MovieService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "YOUR_API_KEY"; // Replace with your actual key

    private TmdbResponse mockData = new TmdbResponse
    {
        Results = new List<MovieDto>
        {
            new MovieDto
            {
                Title = "Spider-Man",
                Overview = "After being bitten by a genetically altered spider, Peter Parker gains spider-like abilities.",
                PosterPath = "/rweIrveL43TaxUN0akQEaAXL6x0.jpg",
                ReleaseDate = "2002-05-03"
            },
            new MovieDto
            {
                Title = "Spider-Man 2",
                Overview = "Peter Parker struggles to balance his life as a college student and his responsibilities as Spider-Man.",
                PosterPath = "/olxpyq9kJAZ2NU1siLshhhXEPR1.jpg",
                ReleaseDate = "2004-06-30"
            },
            new MovieDto
            {
                Title = "Spider-Man 3",
                Overview = "Peter Parker faces new challenges as he battles multiple villains and his own inner darkness.",
                PosterPath = "/qFmwhVUoUSXjkKR9kQ6clHXYObG.jpg",
                ReleaseDate = "2007-05-04"
            }
        }
    };

    public MovieService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<MovieDto>> SearchMoviesAsync(string query)
    {
        // var response = await _httpClient.GetAsync($"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={query}");
        // response.EnsureSuccessStatusCode();

        // var json = await response.Content.ReadAsStringAsync();
        // var results = JsonConvert.DeserializeObject<TmdbResponse>(json);
        var searchedResults = mockData.Results.FindAll((movie) => movie.Title.Contains(query)).ToList<MovieDto>();
        return searchedResults;
    }
}



public class TmdbResponse
{
    public List<MovieDto> Results { get; set; }
}
