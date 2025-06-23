# Insightify: Serverless NLP Analysis Pipeline

Insightify is a highly scalable, event-driven **serverless** application built on **AWS** and **.NET 8**. It performs Natural Language Processing (NLP) on user-submitted text, detecting sentiment and key entities. The decoupled architecture ensures resilience and asynchronous processing for a seamless user experience.

---

## Key Features

-   **Asynchronous NLP Processing** – Jobs are submitted and processed asynchronously, so long-running analysis never blocks the user.
-   **Sentiment & Entity Detection** – Uses Amazon Comprehend for sophisticated sentiment analysis and entity recognition.
-   **Secure & Scalable API** – REST endpoints exposed through Amazon API Gateway for submitting jobs and retrieving results.
-   **Low-Latency Status Tracking** – DynamoDB stores job status and NLP results with minimal latency.
-   **Infrastructure as Code** – 20+ AWS resources version-controlled and automated with AWS SAM.
-   **Custom Domain & HTTPS** – Static frontend on S3, delivered via CloudFront, secured by ACM and managed with Route 53.

---

## Architecture

Insightify follows an event-driven, serverless design for maximum scalability and cost efficiency:

1.  A user submits text through the static site on Amazon S3 (served via CloudFront).
2.  The request hits a secure API Gateway endpoint that invokes the **Submit** Lambda.
3.  **Submit** enqueues the job on **Amazon SQS** and immediately returns a `jobId`.
4.  The **Process** Lambda is triggered by SQS, calls **Amazon Comprehend** for analysis, then writes results and status to **DynamoDB**.
5.  Clients poll `GET /jobs/{jobId}` to retrieve results.

*Tip: create a simple diagram in diagrams.net and link it here.*

---

## Technology Stack

### Backend
-   **Runtime:** C# / .NET 8 
-   **Framework:** ASP.NET Core for Lambda 
-   **Concepts:** RESTful APIs , Event-Driven Design 

### Cloud & DevOps
-   **Compute:** AWS Lambda 
-   **Messaging:** Amazon SQS 
-   **Database:** Amazon DynamoDB 
-   **API:** Amazon API Gateway 
-   **NLP:** Amazon Comprehend 
-   **Storage & Hosting:** Amazon S3 
-   **CDN:** Amazon CloudFront 
-   **DNS & TLS:** Amazon Route 53, AWS Certificate Manager 
-   **IaC:** AWS SAM (Serverless Application Model)

### Frontend
-   Static site hosted on Amazon S3 

---

## Setup & Deployment

The entire stack is defined in **`template.yaml`** (AWS SAM).

### Prerequisites

-   AWS account
-   **AWS CLI** configured
-   **AWS SAM CLI** installed
-   **.NET 8 SDK** 

### Clone

```bash
git clone [https://github.com/ymekonnen9/Insightify.git](https://github.com/ymekonnen9/Insightify.git)
```bash
sam build
```bash
sam deploy --guided
