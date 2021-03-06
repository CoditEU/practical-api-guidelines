# Practical API Guidelines - Must have

1. [General HTTP Guidelines](#general-http-guidelines)
   -  [URL Naming](#url-naming)
   -  [Versioning](#versioning)
   -  [Data Contracts](#data-contracts)
   -  [HTTP Methods](#http-methods)
   -  [HTTP Status Codes](#http-status-codes
   )
2. [Security-first](#security-first)
3. [Error Handling](#error-handling)
4. [Document your APIs](#document-your-apis)
   - [Defining OperationIds](#defining-operationids)
   - [Generating OpenAPI Documentation](#generating-openapi-documentation)
5. [Unit and Integration tests](#unit-and-integratiom-tests)

## General HTTP Guidelines
### URL Naming
Think about your operation URIs and make them as RESTy as possible – What we mean with RESTy?
- **API name is a singular concept**
   - _Do - use `order` in the name of the API_
     - _`https://order.contoso.com/v1/{controller}`_
     - _`https://api.contoso.com/order/v1/{controller}`_
   - _Don't - use `orders` in the name of the API_
     - _`https://orders.contoso.com/v1/{controller}`_
     - _`https://api.contoso.com/orders/v1/{controller}`_
- **Only use api once in URLs, unless there is a need for it**
   - You should not duplicate information if there is no need for it. If API is only a segment of the URL, such as frontend and api, then it is ok to share them.
   - _Do_
     - _`https://customer.contoso.com/api/v1/{controller}`_
     - _`https://api.domain.com/hr/v1/{controller}`_   
   - _Don't_
     - _Avoid duplication - `https://api.contoso.com/api/customer/v1/{controller}`_
     - _No added value - `https://data.contoso.com/api/customer/v1/{controller}`_
- **Use plural for collections of resources**
   - _Example -`https://api.contoso.com/master-data/items`_
- **Don't use verbs in the url**
   - _Do - `https://api.contoso.com/master-data/items`_
   - _Don't - `https://api.contoso.com/master-data/getItems`_
- **Always use lower case in uris**
   - _Do - `https://api.contoso.com/file-ingestion/{collection}/{blob}`_
   - _Don't - `https://api.contoso.com/File-Ingestion/{collection}/{blob}`_
- **Use the `-` hyphen-character for better readability**
   - _Do - `https://api.contoso.com/technical-account/contracts`_
   - _Don't - `https://api.contoso.com/technicalaccount/contracts`_
- **Command can be part of the uri path**
   - _Example - `http://api.example.com/cart-management/users/{id}/carts/{id}/checkout`_
- **Query strings should only be used for querying (ie filtering, paging, etc..) and not for actions nor commands.**
   - _Do - `POST https://api.contoso.com/master-data/contracts/{contractId}/cancel`_
   - _Don't - `GET https://api.contoso.com/master-data/contracts/{contractId}?operation=cancel`_

### Versioning

API versioning supports four methods of versioning out-of-the-box:  

| Versioning  | Example    | Notes |
|:--------|:-------------------|:-----|
| by URL segment | https://api.contoso.com/master-data/v1/items/{itemId}| This URI-based resource versioning doesn't support a default or implicit matching of the current version. On the other hand is the simplest  approach, the less error prone therefore the most used.|
| by querystring |https://api.contoso.com/master-data/v1/items/{itemId}?version=1.0|Query strings, unlikely URL segments, can have default values, so it easily support default versioning. If clients don't request a specific version, should they get the earliest or the latest version?|
| by media type (Resource versioning) |Accept: application/vnd.mdm.v2+json|The most RESTful approach but the most complex one for consumers especially when the header is optional.|
| by header |Version:1.0|Less used, with the header approach you do not mix up version parameters with resource filters as with querystring.| 

Versioning is highly debated topic and for sure there is no 'right way' to version your APIs.

- **Version your endpoint** even if you don’t need it (yet)
   - Allows you to introduce new versions later one without breaking anything
   - Path versioning (by URL segment) is the advised approach because of its simplicity. 
     - Example - `https://api.contoso.com/master-data/v1/items/{itemId}/components/{componentId} 
   - Think how your api will evolve avoinding breaking changes before picking up a versioning style.
- **Data contract versioning is determined by [using content-negotiation and custom media types](https://github.com/Microsoft/aspnet-api-versioning/wiki/Versioning-by-Media-Type)**
   - _Example - `GET /items/H12652 Accept: application/vnd.mdm.v2+json`_

### Data Contracts

- Use camelCase for the attributes
- Use an [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) notation for formatting datetimes, such as `2018-06-17T20:30:12.511Z`
- Avoid using abbreviations
- Serialize enumerations to strings

Example:
```json
{
  "customer": {
    "firstName": "Tom",
    "lastName": "Kerkhove",
    "createdOn": "2018-06-17T20:30:12.511Z"
  },
  "price": 100.0,
  "currency": "Eur"
}
```

### HTTP Methods

| Method  | Idempotent (\*)    | Safe (\*\*)        | When to use | Notes |
|:--------|:-------------------|:-------------------|:------------|:------|
| GET     | :heavy_check_mark: | :heavy_check_mark: | Getting the current resource or a list of resources | |
| POST    | :x:                | :x:                | Create a new resource, execute a command (e.g. cart checkout) | If you are not a REST extremist: It can also be used to send a complex query and GET a query result. |
| PUT     | :heavy_check_mark: | :x:                | This should be used to update the whole object | This is not OK for incremental updates. |
| PATCH   | :x:                | :x:                | Incremental update | If you want to update a limited number of fields. See [json-patch](https://tools.ietf.org/html/rfc6902) and [merge-patch](https://tools.ietf.org/html/rfc7386)  |
| DELETE  | :heavy_check_mark: | :x:                | Hard/Soft delete | |
| OPTIONS | :heavy_check_mark: | :heavy_check_mark: | Mainly used for CORS | |

*(\*) From a RESTful service standpoint, for an operation (or service call) to be idempotent, clients can make that same call repeatedly while producing the same result. In other words, making multiple identical requests has the same effect as making a single request. Note that while idempotent operations produce the same result on the server (no side effects), the response itself may not be the same (e.g. a resource's state may change between requests).*

*(\*\*) Safe methods are HTTP methods that do not modify resources. For instance, using GET or HEAD on a resource URL, should NEVER change the resource. However, this is not completely true. It means: it won't change the resource representation. It is still possible, that safe methods do change things on a server or resource, but this should not reflect in a different representation.*

### HTTP Status Codes

Some notes about the most used HTTP status codes.

| Code  | Description (\*)    | When to use        | Notes | 
|:--------|:-------------------|:-------------------|:------------|
| 200     | Success | to GET a resource, to update (PUT/PATCH) a resource, when you use a POST to perform a complex SEARCH. | 200 always return a BODY, if not use 204 - No Content. Sometimes a 200 is also returned when a POST is used to create a resource even if the best status code should be 201. |
| 201    | Created | create a resource using POST | 201 always return a body. The 201 response payload typically return the instance created or describes and links to the resource(s) created. Sometimes the link to the created resource is also provided via the Location header. |
| 202    | Accepted | POST an async request | The asynchronous operation APIs frequently use the 202 Accepted status code and Location headers to indicate progress. You can also use this to create different resources of a batch process. |
| 204    | No Content | normally used with a POST to execute a command | The server has successfully fulfilled the request and that there is no additional content to send in the response payload body. |
| 400     | Bad Request | Data issues such as invalid JSON, etc. | The server cannot or will not process the request due to something that is perceived to be a client error |
| 401   | Unauthorized | use when the Authentication/Authorization is required but has failed | Do not confuse this one with 403.  |
| 403   | Forbidden                | The action is not allowed. For example you're trying to get access to a resource that belong to another user. | For example, you are calling an existing resource passing the right credentials, but you dont have the right authorization. |
| 404   | Not Found                | When you request a resource that is not present |  |
| 409  | Conflict | You run an update that brings the resource to an inconsistent state. | Duplicate data or invalid data state. | 
| 500 | Internal Server Error | The request has been accepted but there is something wrong with your code | Fix your code ;) | 

NOTE: If there isn't a good 4XX code use 400, if there isn't a good 5XX code, use 500, isn't a good 2XX use 200 and return more precise details in the body.
*(\*) Do not use any custom Reason Phrase (Status Description) in your API because it's not supported anymore with HTTP/2.*

## Security-first

- Always use HTTPS, unless otherwise required ([official documentation](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1))
  - In case HTTP should be supported, consider using an API gateway in front of the API.
This allows you to be secure on the physical API while the consumers can still use HTTP
- Do not put security keys and sensitive information in the query string
  - Certain scenarios are exceptional such as exposing webhooks. When this is the case the keys need to be limited in time to live.
- Frontdoor/Backdoor security will be addressed in the next maturity levels. Authentication / Authorization can be delagated to gateway components.

## Error Handling

- Use a global exception handler which allows you to track & handle unhandled exceptions very easily
- Errors should be propogated in a consistent way
  - Use `application/problem+json` following [RFC 7807](https://tools.ietf.org/html/rfc7807).
    - Every 4XX/5XX should  the same data contract
    - Less details compared to a custom data contract
    - Read [this blog post](https://tech.domain.com.au/2017/11/please-dont-spare-me-the-details/) on how to achieve this
  - If the above suggestion is not possible you should use a custom data contract. See Microsoft example [here](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#7102-error-condition-responses).
- Shouldn't there be a general response for our API's, a general scheme we can use after all? Maybe using trackingcodes etc with map to app insights?

## Document your APIs
Document your API and be as descriptive as possible – New people should get a clear understanding of what they can expect.

Documentation should provide information about the following at least:
- OperationId
- General Description
- Parameters
- Response codes & contracts

### Defining OperationIds
An operationId is a unique identifier for an operation that is provided on an API. It is important to think carefully when assigning an operationId as changing these later on will be a breaking change.

OpenAPI tooling, such as [AutoRest](https://github.com/Azure/autorest) & [NSwag](https://github.com/RSuter/NSwag), use the operationId to generate API clients based on this convention so it is important to provide a descriptive operationId. In order to ease the use of your API we recommend using the `{controller}_{operationName}` pattern in pascal casing.

### Generating OpenAPI Documentation
Every API should have documentation in the OpenAPI format. If you want to generate those based on your code you can use tools like [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle) & [NSwag](https://github.com/RSuter/NSwag).

Here is an example on how to generate them with Swashbuckle
```csharp
        /// <summary>
        /// Get Player Profiles
        /// </summary>
        /// <param name="topPlayersOnly">Indicates whether to return the top players only</param>
        /// <remarks>Provides a profile for all known players</remarks>
        /// <returns>Return a list of Players</returns>
        [HttpGet(Name = "Players_GetPlayers")]
        [SwaggerResponse((int)HttpStatusCode.OK, "List of players")]
        [SwaggerResponse((int) HttpStatusCode.BadRequest, "Input validation error.")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, "API is not available")]
        public IHttpActionResult GetPlayers(bool topPlayersOnly)	
        {	
            return Ok();	
        }
      
```
How xml comments and attributes are mapping to the OpenApi output.

`Summary` should provide a brief and clear name for the operation where `Remarks` provides a well-written business description of what the operation does and should not leak any technical information.

| Type | Value | Open Api field | Notes |
|:--------|:-------------------|:-------------------|:------|
| XML comment | Summary | Summary | Short description. Human readable, business oriented |
| XML comment| Remarks | Description | Long description. Human readble, business oriented |
| XML comment | Returns | N/A | Not used |
| XML comment | Param name | Parameters/Name | - |
| XML comment | Param value | Parameters/Description | - |
| Http Attribute | Name | OperationId | - |
| SwaggerResponse Attribute | StatusCode | Responses/{StatusCode} | For all controllers, document all possible return status codes |
| SwaggerResponse Attribute | Description | Responses/Description |  Technical description. Describe whether a body is returned and any particular header.|
| Required Attribute | Required | N/A | Used to determine if parameters/fields are required or optional |

## Unit and Integration tests
Always add Unit tests and Integration tests.
- Put as less as possible logic in the controller and enhance testing possibilities through dependeny injection.
- Tests all the routes, status codes and content-types.
- Tests the generated Open API document.
