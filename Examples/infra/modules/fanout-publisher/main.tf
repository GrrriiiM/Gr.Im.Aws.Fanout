resource "aws_sns_topic" "fanout" {
  name = "${var.publisher_name}-fanout"
  # fifo_topic = true
}



