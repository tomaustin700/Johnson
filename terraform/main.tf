terraform {
  backend "azurerm" {
    resource_group_name  = "Johnson"
    storage_account_name = "johnsonterraform"
    container_name       = "tstate"
    key                  = "johnson.terraform.tfstate"

  }

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3"
    }
  }

}


provider "azurerm" {
  features {}
  skip_provider_registration = true
}

data "azurerm_client_config" "current" {}

variable "access_token" {}
variable "access_token_secret" {}
variable "api_key" {}
variable "api_key_secret" {}

variable "resource_group" {
  type    = string
  default = "Johnson"
}
variable "location" {
  type    = string
  default = "uksouth"
}


resource "azurerm_storage_account" "functionstorage" {
  name                     = "jstorage"
  resource_group_name      = var.resource_group
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}


resource "azurerm_service_plan" "serviceplan" {
  name                = "jsp"
  resource_group_name = var.resource_group
  location            = var.location
  os_type             = "Windows"
  sku_name            = "Y1"
}

resource "azurerm_windows_function_app" "function" {
  name                = "jfunction"
  resource_group_name = var.resource_group
  location            = var.location

  storage_account_name       = azurerm_storage_account.functionstorage.name
  storage_account_access_key = azurerm_storage_account.functionstorage.primary_access_key
  service_plan_id            = azurerm_service_plan.serviceplan.id

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"        = "dotnet",
    "WEBSITE_RUN_FROM_PACKAGE"        = "1",
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE" = "true"
    "AccessToken"                     = var.access_token,
    "AccessTokenSecret"               = var.access_token_secret,
    "APIKey"                          = var.api_key,
    "APIKeySecret"                    = var.api_key_secret,
  }
}



