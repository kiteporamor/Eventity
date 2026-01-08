# language: en
@2FA
Feature: Two-factor authentication and password rotation

  Background:
    Given a technical user exists
    And two-factor authentication is enabled

  Scenario: Email-based 2FA authentication
    When the user attempts to log in
    Then a verification code is required
    And a verification code is delivered via email
    When the user verifies the code
    Then authentication succeeds and a JWT token is issued
    And access to protected resources is granted

  Scenario: Limited 2FA attempts and recovery after lockout
    When the user attempts to log in
    Then a verification code is required
    When the user enters an invalid verification code for the maximum number of attempts
    Then the user is locked out from 2FA verification
    When the lockout period has elapsed
    And the user attempts to log in
    Then a verification code is required
    When the user verifies the code
    Then authentication succeeds and a JWT token is issued

  @PasswordChange
  Scenario: Planned password change with 2FA
    When the user attempts to log in
    Then a verification code is required
    When the user verifies the code
    Then authentication succeeds and a JWT token is issued
    When the user changes the password to a new value
    Then the password change is successful
    When the user attempts to log in with the new password
    Then a verification code is required
    When the user verifies the code
    Then authentication succeeds and a JWT token is issued
