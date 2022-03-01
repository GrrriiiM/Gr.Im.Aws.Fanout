terraform {
  required_providers {
    aws = {
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region                      = "us-east-1"
  access_key                  = "fake"
  secret_key                  = "fake"
  skip_credentials_validation = true
  skip_metadata_api_check     = true
  skip_requesting_account_id  = true
  s3_force_path_style         = true


  endpoints {
    sns = "http://localhost:4566"
    sqs = "http://localhost:4566"
  }
}

resource "aws_sns_topic" "teste-fan-out" {
  name = "teste-fan-out"
}

resource "aws_sqs_queue" "teste-fan-out" {
  name       = "teste-fan-out.fifo"
  fifo_queue = true
}


resource "aws_sns_topic_subscription" "teste-fan-out" {
  
  protocol      = "sqs"
  endpoint      = aws_sqs_queue.teste-fan-out.arn
  topic_arn     = aws_sns_topic.teste-fan-out.arn
  filter_policy = <<EOF
  {
    "message-name": [
        "message-123",
        "message-321"
    ]
  }
  EOF
}

# data "aws_iam_policy_document" "teste-fan-out" {
#   policy_id = "teste-fan-out"
#   statement {
#     effect = "Allow"
#     actions = [
#       "SNS:Subscribe",
#       "SNS:Receive",
#     ]

#     condition {
#       test     = "StringLike"
#       variable = "SNS:Endpoint"
#       values = [
#         "arn:aws:sqs:*:*:teste-fan-out",
#       ]
#     }

#     principals {
#       type        = "AWS"
#       identifiers = ["*"]
#     }

#     resources = [
#       "arn:aws:sns:*:*:teste-fan-out",
#     ]
#   }
# }
