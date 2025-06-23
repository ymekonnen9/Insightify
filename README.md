Insightify: Serverless NLP Analysis Pipeline



Insightify is a highly scalable, event-driven serverless application built on AWS and .NET 8. It performs Natural Language Processing (NLP) analysis on user-submitted text, detecting sentiment and key entities. The decoupled architecture ensures resilience and asynchronous processing for a seamless user experience.

Live Demo: https://insightify.yaredmekonnendomain.click

Key Features
Asynchronous NLP Processing: Jobs are submitted and processed asynchronously, allowing the system to handle long-running analysis tasks without blocking the user. 
Sentiment & Entity Detection: Leverages Amazon Comprehend to perform sophisticated sentiment analysis and entity recognition on text. 
Secure & Scalable API: Exposes a secure REST API through Amazon API Gateway for submitting jobs and retrieving results. 
Low-Latency Status Tracking: Utilizes DynamoDB to store and retrieve job status and NLP results with minimal latency. 
Infrastructure as Code (IaC): The entire cloud infrastructure, including over 20 unique resources, is version-controlled and automated using AWS SAM.
Custom Domain & HTTPS: The frontend is hosted on S3 and delivered globally via CloudFront with a custom domain configured through Route 53 and ACM for security. 
Architecture
Insightify is architected as an event-driven, serverless application on AWS to ensure scalability and cost-efficiency. 

The workflow is as follows:

A user submits text through the static frontend hosted on Amazon S3 and delivered by CloudFront.
The request hits a secure API Gateway endpoint, which triggers a "Submit" AWS Lambda function.
The "Submit" Lambda places the job details onto an SQS queue for reliable, decoupled processing and immediately returns a job ID to the user. 
A "Process" AWS Lambda function is triggered by messages appearing in the SQS queue. 
This "Process" function calls the Amazon Comprehend service to perform the core sentiment and entity analysis. 
Once the analysis is complete, the results and job status are stored in a DynamoDB table for fast, persistent storage and retrieval. 
You can create a simple diagram using a tool like diagrams.net and link it here.

Technology Stack
Backend
Runtime: C# / .NET 8
Framework: ASP.NET Core for Lambda
Concepts: RESTful APIs, Clean Architecture, Event-Driven Design
Cloud & DevOps
Compute: AWS Lambda 
Messaging: AWS SQS (Simple Queue Service) 
Database: Amazon DynamoDB 
API: Amazon API Gateway 
NLP Service: Amazon Comprehend 
Storage & Hosting: Amazon S3 
CDN: Amazon CloudFront 
DNS & Security: Amazon Route 53, AWS Certificate Manager (ACM) 
IaC: AWS SAM (Serverless Application Model)
Frontend
Hosting: Static site hosted on Amazon S3 
Setup and Deployment
The entire infrastructure for this project is defined in an AWS SAM template.yaml file.

To deploy this stack:

Prerequisites:

An AWS Account
AWS CLI installed and configured
AWS SAM CLI installed
.NET 8 SDK
Clone the repository:

Bash

git clone https://github.com/ymekonnen9/Insightify.git
cd Insightify
Build the SAM application:

Bash

sam build
Deploy to AWS:

Bash

sam deploy --guided
Follow the on-screen prompts to deploy the serverless application to your AWS account. The SAM CLI will provision all the necessary resources as defined in the template.
