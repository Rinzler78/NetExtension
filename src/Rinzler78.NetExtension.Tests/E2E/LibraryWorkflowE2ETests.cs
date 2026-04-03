using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Strings;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.E2E;

[Trait("Category", "E2E")]
public class LibraryWorkflowE2ETests
{
    private sealed class WorkflowDocument : ObservableObject
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private List<string> _tags = new();

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public List<string> Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public string Slug => Title.ToPascalCase();
    }

    [Fact]
    public void EndToEnd_ObservableJsonFileWorkflow_ShouldRoundTripSuccessfully()
    {
        var filePath = MockHelpers.CreateTempFile("", ".json");

        try
        {
            var propertyChanges = new List<string>();
            var document = new WorkflowDocument();
            document.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is not null)
                {
                    propertyChanges.Add(args.PropertyName);
                }
            };

            document.Title = "net extension coverage plan";
            document.Description = "QUALITY GATE FOR LIBRARY WORKFLOWS".ToStartByUpperCase();
            document.Tags = new List<string> { "coverage", "integration-tests", "e2e-suite" }
                .Select(tag => tag.ToPascalCase())
                .ToList();

            var json = document.SerializeObject(Formatting.Indented);
            File.WriteAllText(filePath, json);

            var restored = filePath.DeserializeObjectFromFile<WorkflowDocument>();

            restored.Should().NotBeNull();
            restored!.Title.Should().Be("net extension coverage plan");
            restored.Description.Should().Be("Quality gate for library workflows");
            restored.Slug.Should().Be("NetExtensionCoveragePlan");
            restored.Tags.Should().BeEquivalentTo(new[] { "Coverage", "IntegrationTests", "E2ESuite" });
            propertyChanges.Should().Contain(new[] { "Title", "Description", "Tags" });
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    [Fact]
    public void EndToEnd_StringValidationTransformationAndSerialization_ShouldProduceConsumablePayload()
    {
        var records = new[]
        {
            new { Name = "john doe", Email = "john.doe@example.com", Labels = new[] { "first-pass", "core_user" } },
            new { Name = "invalid account", Email = "not-an-email", Labels = new[] { "reject", "bad_data" } },
            new { Name = "alice smith", Email = "alice.smith@example.com", Labels = new[] { "vip_user", "beta-group" } }
        };

        var payload = records
            .Where(record => record.Email.IsValidEmail())
            .Select(record => new
            {
                DisplayName = record.Name.ToPascalCase(),
                record.Email,
                Labels = record.Labels.Select(label => label.ToPascalCase()).ToArray(),
                Similarity = record.Email.CalculateSimilarity("test@example.com")
            })
            .OrderByDescending(record => record.Similarity)
            .ToList();

        var json = payload.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<Dictionary<string, object>>>();

        payload.Should().HaveCount(2);
        payload.Select(item => item.DisplayName).Should().BeEquivalentTo(new[] { "JohnDoe", "AliceSmith" });
        json.Should().Contain("john.doe@example.com");
        json.Should().Contain("alice.smith@example.com");
        restored.Should().NotBeNull();
        restored.Should().HaveCount(2);
    }
}
