# AdobeDocumentServicesClient

The AdobeClientConnector library provides an interface for integrating with Adobe Document Services API to perform tasks such as token management, document merging, and file uploads.

## Features

* Authenticate with Adobe APIs using OAuth2.
* Upload documents to Adobe services.
* Merge Word templates with JSON data to generate PDFs.
* Monitor file processing status.
* Download processed files from Adobe servers.

## Table of Contents

1. Installation
2. Configuration
3. Usage
4. Dependency Injection
5. Set Adobe Region URL
6. Merge Document
7. API Reference
8. Error Handling
9. Logging

## Installation

Add the library to your project via a NuGet package (if applicable):

`Install-Package AdobeClientConnector`

### Configuration

To use the AdobeClientConnector service, ensure the following configurations are present in your appsettings.json:

`{
  "AdobeCredentials": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "Region": "UnitedStates" // Options: UnitedStates, Europe, Custom
  }
}`

## Usage

### Dependency Injection

The library integrates with .NET dependency injection. Register it in your Startup.cs or Program.cs:

`services.Configure<AdobeCredentials>(configuration.GetSection("AdobeCredentials"));
services.AddHttpClient();
services.AddTransient<IAdobeClientConnector, AdobeClientConnector>();`

### Override Region if not set up in Config:

`adobeClientConnector.SetAdobeURLRegion(RegionOptions.UnitedStates);`

### Override Adobe's API URL with custom URL:

`adobeClientConnector.SetAdobeURLRegion("https://custom-adobe-api-url.com");`

### Merge Document

To merge a Word template with JSON data and generate a PDF:

`var wordTemplateStream = File.OpenRead("template.docx");
var jsonData = JObject.Parse(@"{ 'Name': 'John Doe', 'Age': 30 }");

var resultStream = await adobeClientConnector.MergeDocumentAsync(wordTemplateStream, jsonData);

using var fileStream = File.Create("output.pdf");
await resultStream.CopyToAsync(fileStream);`

## API Reference

### Public Methods

> SetAdobeURLRegion(RegionOptions regionOptions)

Sets the API base URL based on the specified region.

> SetAdobeURLRegion(string customUrl)

Sets a custom API base URL.

> MergeDocumentAsync(Stream wordTemplate, JObject jsonObject)

Merges a Word document with JSON data to generate a PDF.

> GetTokenAsync()

Retrieves an OAuth2 token from Adobe API.

> GetUploadPreSignedUriAsync(AdobeToken adobeToken)

Generates a pre-signed URL for uploading documents.

> UploadDocumentAsync(string uploadUri, Stream wordTemplate)

Uploads a document to the specified Adobe pre-signed URL.

> DocumentGenerationAsync(JObject jsonObject, AdobeToken adobeToken, string assetId)

Generates a document based on the provided JSON data and asset ID.

> CheckFileStatusAsync(AdobeToken adobeToken, MergeDocumentResponse documentResponse)

Checks the status of a file processing request.

> GetFileStreamAsync(string fileUrl)

Downloads a file from Adobe services as a stream.

## Error Handling

Exceptions are logged using the provided ILogger instance. Key exception types include:

* ValidationException: Raised for invalid constructor parameters.
* UnauthorizedAccessException: Raised when token retrieval fails.
* ArgumentNullException: Raised for null input streams or parameters.
* HttpRequestException: Raised for failed HTTP requests.

## Logging

The library logs requests, responses, and errors using the ILogger interface. Example:

`_logger.LogInformation("Request URL: {Url}", uploadUri);
_logger.LogError(ex, "An error occurred while merging documents.");`


