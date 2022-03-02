module "fanout-publisher-teste" {
  source         = "./modules/fanout-publisher"
  publisher_name = "teste1"
}

module "fanout-subscriber-teste" {
  source            = "./modules/fanout-subscriber"
  subscriber_name   = "teste2"
  publisher_name    = "teste1"
  publisher_region  = var.region
  publisher_account = var.account
  messages_filter = [
    "mensagem-123",
    "mensagem-321",
    "mensagem-1234"
  ]
}


# resource "aws_sns_topic" "teste-fan-out" {
#   name = "teste-fan-out"
# }

# resource "aws_sqs_queue" "teste-fan-out" {
#   name       = "teste-fan-out.fifo"
#   fifo_queue = true
# }


# resource "aws_sns_topic_subscription" "teste-fan-out" {

#   protocol      = "sqs"
#   endpoint      = aws_sqs_queue.teste-fan-out.arn
#   topic_arn     = aws_sns_topic.teste-fan-out.arn
#   filter_policy = <<EOF
#   {
#     "message-name": [
#         "message-123",
#         "message-321"
#     ]
#   }
#   EOF
# }

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
