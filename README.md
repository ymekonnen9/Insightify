# Insightify: Serverless NLP Analysis Pipeline

[cite_start]Insightify is a highly scalable, event-driven **serverless** application built on **AWS** and **.NET 8**. [cite_start]It performs Natural Language Processing (NLP) on user-submitted text, detecting sentiment and key entities. [cite_start]The decoupled architecture ensures resilience and asynchronous processing for a seamless user experience.

[cite_start]**Live demo:** <https://insightify.yaredmekonnendomain.click> 

---

## Key Features

-   [cite_start]**Asynchronous NLP Processing** – Jobs are submitted and processed asynchronously, so long-running analysis never blocks the user.
-   [cite_start]**Sentiment & Entity Detection** – Uses Amazon Comprehend for sophisticated sentiment analysis and entity recognition.
-   [cite_start]**Secure & Scalable API** – REST endpoints exposed through Amazon API Gateway for submitting jobs and retrieving results.
-   [cite_start]**Low-Latency Status Tracking** – DynamoDB stores job status and NLP results with minimal latency.
-   **Infrastructure as Code** – 20+ AWS resources version-controlled and automated with AWS SAM.
-   [cite_start]**Custom Domain & HTTPS** – Static frontend on S3, delivered via CloudFront, secured by ACM and managed with Route 53.

---

## Architecture

[cite_start]Insightify follows an event-driven, serverless design for maximum scalability and cost efficiency:

1.  [cite_start]A user submits text through the static site on Amazon S3 (served via CloudFront).
2.  [cite_start]The request hits a secure API Gateway endpoint that invokes the **Submit** Lambda.
3.  [cite_start]**Submit** enqueues the job on **Amazon SQS** and immediately returns a `jobId`.
4.  [cite_start]The **Process** Lambda is triggered by SQS, calls **Amazon Comprehend** for analysis, then writes results and status to **DynamoDB**.
5.  Clients poll `GET /jobs/{jobId}` to retrieve results.

*Tip: create a simple diagram in diagrams.net and link it here.*

---

## Technology Stack

### Backend
-   [cite_start]**Runtime:** C# / .NET 8 
-   [cite_start]**Framework:** ASP.NET Core for Lambda 
-   [cite_start]**Concepts:** RESTful APIs [cite: 4][cite_start], Event-Driven Design 

### Cloud & DevOps
-   [cite_start]**Compute:** AWS Lambda 
-   [cite_start]**Messaging:** Amazon SQS 
-   [cite_start]**Database:** Amazon DynamoDB 
-   [cite_start]**API:** Amazon API Gateway 
-   [cite_start]**NLP:** Amazon Comprehend 
-   [cite_start]**Storage & Hosting:** Amazon S3 
-   [cite_start]**CDN:** Amazon CloudFront 
-   [cite_start]**DNS & TLS:** Amazon Route 53, AWS Certificate Manager 
-   **IaC:** AWS SAM (Serverless Application Model)

### Frontend
-   [cite_start]Static site hosted on Amazon S3 

---

## Setup & Deployment

The entire stack is defined in **`template.yaml`** (AWS SAM).

### Prerequisites

-   AWS account
-   **AWS CLI** configured
-   **AWS SAM CLI** installed
-   [cite_start]**.NET 8 SDK** 

### Clone

```bash
git clone [https://github.com/ymekonnen9/Insightify.git](https://github.com/ymekonnen9/Insightify.git)
```bash
sam build
```bash
sam deploy --guided
