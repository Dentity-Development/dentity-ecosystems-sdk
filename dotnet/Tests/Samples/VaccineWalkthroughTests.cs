using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Dentity;
using Dentity.Sdk.Options.V1;
using Dentity.Services.Common.V1;
using Dentity.Services.VerifiableCredentials.Templates.V1;
using Xunit;
using Xunit.Abstractions;
using JsonSerializer = System.Text.Json.JsonSerializer;

#pragma warning disable CS0618

namespace Tests;

[SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
public class VaccineWalkthroughTests
{

    private readonly ITestOutputHelper _testOutputHelper;
    private readonly DentityOptions _options;

    public VaccineWalkthroughTests(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;

        _options = new() {
            ServerEndpoint = Environment.GetEnvironmentVariable("TEST_SERVER_ENDPOINT") ?? Tests.DefaultEndpoint,
            ServerPort = int.TryParse(Environment.GetEnvironmentVariable("TEST_SERVER_PORT"), out var port) ? port : Tests.DefaultPort,
            ServerUseTls = !bool.TryParse(Environment.GetEnvironmentVariable("TEST_SERVER_USE_TLS"), out var tls) || tls
        };

        _testOutputHelper.WriteLine($"Testing endpoint: {_options.FormatUrl()}");
    }

    [Fact(DisplayName = "Vaccine Walkthrough")]
    public async Task TestWalkthrough() {
        // createEcosystem() {
        var trinsic = new DentityService(_options);
        var (ecosystem, _) = await trinsic.Provider.CreateEcosystemAsync(new());
        var ecosystemId = ecosystem?.Id;
        // }

        ecosystemId.Should().NotBeNullOrEmpty();

        // setupActors() {
        var allison = await trinsic.Wallet.CreateWalletAsync(new() { EcosystemId = ecosystemId! });
        var clinic = await trinsic.Wallet.CreateWalletAsync(new() { EcosystemId = ecosystemId! });
        var airline = await trinsic.Wallet.CreateWalletAsync(new() { EcosystemId = ecosystemId! });
        // }

        allison.AuthToken.Should().NotBeNullOrEmpty();
        clinic.AuthToken.Should().NotBeNullOrEmpty();
        airline.AuthToken.Should().NotBeNullOrEmpty();

        // createTemplate() {
        // Set active profile to `clinic` so we can create a template
        trinsic = new DentityService(_options.CloneWithAuthToken(clinic.AuthToken!));

        // Prepare request to create template
        CreateCredentialTemplateRequest templateRequest = new() {
            Name = "VaccinationCertificate",
            AllowAdditionalFields = false
        };

        templateRequest.Fields.Add("firstName", new() { Description = "First name of vaccine recipient" });
        templateRequest.Fields.Add("lastName", new() { Description = "Last name of vaccine recipient" });
        templateRequest.Fields.Add("batchNumber", new() { Description = "Batch number of vaccine", Type = FieldType.String });
        templateRequest.Fields.Add("countryOfVaccination", new() { Description = "Country in which the subject was vaccinated" });

        // Create template
        var template = await trinsic.Template.CreateAsync(templateRequest);
        var templateId = template?.Data?.Id;
        // }

        templateId.Should().NotBeNullOrEmpty();

        // issueCredential() {
        // Prepare credential values
        var credentialValues = new Dictionary<string, string> {
            { "firstName", "Allison" },
            { "lastName", "Allisonne" },
            { "batchNumber", "123454321" },
            { "countryOfVaccination", "US" }
        };

        // Issue credential as clinic
        var issueResponse = await trinsic.Credential.IssueFromTemplateAsync(new() {
            TemplateId = templateId,
            ValuesJson = JsonSerializer.Serialize(credentialValues)
        });

        var signedCredential = issueResponse?.DocumentJson;
        // }

        signedCredential.Should().NotBeNullOrEmpty();

        // storeCredential() {
        // Set active profile to 'allison' so we can manage her cloud wallet
        trinsic = new DentityService(_options.CloneWithAuthToken(allison.AuthToken!));

        // Insert credential into Allison's wallet
        var insertItemResponse = await trinsic.Wallet.InsertItemAsync(new() {
            ItemJson = signedCredential
        });

        var itemId = insertItemResponse?.ItemId;
        // }

        itemId.Should().NotBeNullOrEmpty();

        // shareCredential() {
        // Build a proof for the signed credential as allison
        var proofResponse = await trinsic.Credential.CreateProofAsync(new() {
            ItemId = itemId
        });

        var proofJson = proofResponse?.ProofDocumentJson;
        // }

        proofJson.Should().NotBeNullOrEmpty();

        // verifyCredential() {
        // Set active profile to `airline`
        trinsic = new DentityService(_options.CloneWithAuthToken(airline.AuthToken!));

        // Verify that Allison has provided a valid proof
        var verifyResponse = await trinsic.Credential.VerifyProofAsync(new() {
            ProofDocumentJson = proofJson
        });

        var credentialValid = verifyResponse?.IsValid ?? false;
        // }

        credentialValid.Should().BeTrue();

        _testOutputHelper.WriteLine($"Proof: {proofJson}");
    }
}