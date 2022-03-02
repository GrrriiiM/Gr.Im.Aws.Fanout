# data "aws_iam_policy_document" "fanout" {
#   policy_id = "${var.publisher_name}-fanout-sns-topic-subscription-role"
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
#         "arn:aws:sqs:*:*:${var.subscriber_name}-fanout",
#       ]
#     }

#     principals {
#       type        = "AWS"
#       identifiers = ["*"]
#     }

#     resources = [
#       local.publisher_arn,
#     ]
#   }
# }




resource "aws_sqs_queue" "fanout" {
  name       = "${var.subscriber_name}-fanout.fifo"
  fifo_queue = true
  receive_wait_time_seconds = 10
  message_retention_seconds = 300
}


resource "aws_sns_topic_subscription" "fanout" {
  protocol      = "sqs"
  endpoint      = aws_sqs_queue.fanout.arn
  topic_arn     = local.publisher_arn
  filter_policy = jsonencode({
      message-name = var.messages_filter 
  })
}