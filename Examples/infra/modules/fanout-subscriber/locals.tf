locals {
  publisher_arn = "arn:aws:sns:${var.publisher_region}:${var.publisher_account}:${var.publisher_name}-fanout"
}