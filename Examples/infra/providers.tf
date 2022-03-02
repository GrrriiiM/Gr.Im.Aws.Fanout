provider "aws" {
  region                      = var.region
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