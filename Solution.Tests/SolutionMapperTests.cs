using Solution.SolutionMapper;
using Solution.SolutionMapper.Extensions;
using Mapper = Solution.SolutionMapper.SolutionMapper;

namespace Solution.Tests;

public class SolutionMapperTests
{
    [Fact]
    public void Map_CopiesMatchingPropertiesAndAppliesConfiguredMembers()
    {
        var mapper = new Mapper();
        var configuration = new SolutionMapperConfigurationExpression(mapper);
        configuration.CreateMap<Source, Destination>()
            .ForMember(destination => destination.DisplayName, source => $"{source.FirstName} {source.LastName}");

        var result = mapper.Map<Source, Destination>(new Source
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Age = 36
        });

        Assert.Equal("Ada Lovelace", result.DisplayName);
        Assert.Equal(36, result.Age);
    }

    [Fact]
    public void Map_DoesNotOverwriteIgnoredProperty()
    {
        var mapper = new Mapper();
        var configuration = new SolutionMapperConfigurationExpression(mapper);
        configuration.CreateMap<Source, Destination>().Ignore(destination => destination.Age);

        var result = mapper.Map<Source, Destination>(new Source { Age = 36 });

        Assert.Equal(0, result.Age);
    }

    private sealed class Source
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private sealed class Destination
    {
        public string DisplayName { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
