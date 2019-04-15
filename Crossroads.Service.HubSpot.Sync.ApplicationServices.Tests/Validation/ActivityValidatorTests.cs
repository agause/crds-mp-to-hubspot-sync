﻿using Crossroads.Service.HubSpot.Sync.ApplicationServices.Validation;
using Crossroads.Service.HubSpot.Sync.Data.HubSpot.Models.Response;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing;
using Crossroads.Service.HubSpot.Sync.Data.Mongo.JobProcessing.Dto;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Crossroads.Service.HubSpot.Sync.ApplicationServices.Test.Validation
{
    public class ActivityValidatorTests
    {
        private Activity _fixture;
        private readonly ActivityValidator _validator = new ActivityValidator();
        private readonly SerialSyncFailure _badRequestSerialSyncFailure = new SerialSyncFailure { HttpStatusCode = HttpStatusCode.BadRequest };
        private readonly BulkSyncFailure _badRequestBulkSyncFailure = new BulkSyncFailure { HttpStatusCode = HttpStatusCode.BadRequest };

        public ActivityValidatorTests()
        {
            _fixture = new Activity();
        }

        [Fact]
        public void Activity_WhenNull_ShouldHaveValidationError()
        {
            _validator.ShouldHaveValidationErrorFor(activity => activity, null);
        }

        [Fact]
        public void AllSyncOperations_WhenNull_ShouldNotHaveValidationError()
        {
            // arrange
            var fixture = new Activity {NewRegistrationSyncOperation = null, CoreContactAttributeSyncOperation = null, ChildAgeAndGradeSyncOperation = null};

            // act/assert
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, fixture, ruleSet: RuleSetName.NewRegistrationSync);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, fixture, ruleSet: RuleSetName.CoreContactAttributeSync);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, fixture, ruleSet: RuleSetName.ChildAgeGradeSync);
        }

        [Fact]
        public void AllSyncOperations_WhenNoFailuresExist_ShouldNotHaveValidationError()
        {
            // act/assert
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, _fixture);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, _fixture);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture);
        }

        [Fact]
        public void NewRegistrationSyncOperationActivitySyncResult_WhenHttp400FailuresExist_ShouldNotHaveValidationError()
        {
            NewRegistrationHttp400ShouldNotCauseValidationError(activity => activity.NewRegistrationSyncOperation.SerialCreateResult.Failures);
            NewRegistrationHttp400ShouldNotCauseValidationError(activity => activity.NewRegistrationSyncOperation.SerialUpdateResult.Failures);
            NewRegistrationHttp400ShouldNotCauseValidationError(activity => activity.NewRegistrationSyncOperation.SerialReconciliationResult.Failures);
        }

        private void NewRegistrationHttp400ShouldNotCauseValidationError(Func<IActivity, List<SerialSyncFailure>> failureDetailCollectionSelector)
        {
            // arrange
            _fixture = new Activity();
            failureDetailCollectionSelector(_fixture).Add(_badRequestSerialSyncFailure);

            // act/assert
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, _fixture, RuleSetName.NewRegistrationSync);
        }

        [Fact]
        public void CoreContactAttributeSyncOperationActivitySyncResult_WhenHttp400FailuresExist_ShouldNotHaveValidationError()
        {
            CoreContactAttributeHttp400ShouldNotCauseValidationError(activity => activity.CoreContactAttributeSyncOperation.SerialCreateResult.Failures);
            CoreContactAttributeHttp400ShouldNotCauseValidationError(activity => activity.CoreContactAttributeSyncOperation.SerialUpdateResult.Failures);
            CoreContactAttributeHttp400ShouldNotCauseValidationError(activity => activity.CoreContactAttributeSyncOperation.SerialReconciliationResult.Failures);
        }

        private void CoreContactAttributeHttp400ShouldNotCauseValidationError(Func<IActivity, List<SerialSyncFailure>> failureDetailCollectionSelector)
        {
            // arrange
            _fixture = new Activity();
            failureDetailCollectionSelector(_fixture).Add(_badRequestSerialSyncFailure);

            // act/assert
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, _fixture, RuleSetName.CoreContactAttributeSync);
        }

        [Fact]
        public void ChildAgeAndGradeSyncOperationActivitySyncResult_WhenHttp400FailuresExist_ShouldNotHaveValidationError()
        {
            ChildAgeAndGradeHttp400ShouldNotCauseValidationError(activity => activity.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult1000.FailedBatches);
            ChildAgeAndGradeHttp400ShouldNotCauseValidationError(activity => activity.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult100.FailedBatches);
            ChildAgeAndGradeHttp400ShouldNotCauseValidationError(activity => activity.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult10.FailedBatches);
            ChildAgeAndGradeHttp400ShouldNotCauseValidationError(activity => activity.ChildAgeAndGradeSyncOperation.SerialCreateResult.Failures);
            ChildAgeAndGradeHttp400ShouldNotCauseValidationError(activity => activity.ChildAgeAndGradeSyncOperation.RetryBulkUpdateAsSerialUpdateResult.Failures);
        }

        private void ChildAgeAndGradeHttp400ShouldNotCauseValidationError(Func<IActivity, List<SerialSyncFailure>> failureDetailCollectionSelector)
        {
            _fixture = new Activity();
            failureDetailCollectionSelector(_fixture).Add(_badRequestSerialSyncFailure);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);
        }

        private void ChildAgeAndGradeHttp400ShouldNotCauseValidationError(Func<IActivity, List<BulkSyncFailure>> failureDetailCollectionSelector)
        {
            _fixture = new Activity();
            failureDetailCollectionSelector(_fixture).Add(_badRequestBulkSyncFailure);
            _validator.ShouldNotHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadGateway)]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.ExpectationFailed)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [InlineData(HttpStatusCode.Gone)]
        [InlineData(HttpStatusCode.HttpVersionNotSupported)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.LengthRequired)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.NotAcceptable)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.NotImplemented)]
        [InlineData(HttpStatusCode.PaymentRequired)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [InlineData(HttpStatusCode.ProxyAuthenticationRequired)]
        [InlineData(HttpStatusCode.RequestEntityTooLarge)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.RequestUriTooLong)]
        [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.UnsupportedMediaType)]
        [InlineData(HttpStatusCode.UpgradeRequired)]
        public void AllSyncOperations_When401To599HttpException_ShouldHaveValidationError(HttpStatusCode httpStatusCode)
        {
            VerifyValidationErrorsAreThrown(httpStatusCode: httpStatusCode);
        }

        [Theory]
        [InlineData(ActivityValidator.HubSpotPropertyDoesNotExistSearchString)]
        [InlineData(ActivityValidator.HubSpotPropertyInvalidOptionSearchString)]
        public void AllSyncOperations_WhenValidationResultErrorIsErrorWorthy_ShouldHaveValidationError(string validationResultErrorText)
        {
            VerifyValidationErrorsAreThrown(validationResultErrorText: validationResultErrorText);
        }

        private void VerifyValidationErrorsAreThrown(HttpStatusCode httpStatusCode = HttpStatusCode.OK, string validationResultErrorText = "Value that will never cause validation exception")
        {
            var hubSpotException = new HubSpotException { ValidationResults = new ValidationResult[1] {new ValidationResult {Error = validationResultErrorText}}};
            var four01To599SerialFailure = new SerialSyncFailure { HttpStatusCode = httpStatusCode, Exception = hubSpotException};
            var four01To599BulkFailure = new BulkSyncFailure { HttpStatusCode = httpStatusCode, Exception = hubSpotException };

            _fixture.NewRegistrationSyncOperation.SerialCreateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, _fixture, RuleSetName.NewRegistrationSync);

            _fixture = new Activity();
            _fixture.NewRegistrationSyncOperation.SerialUpdateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, _fixture, RuleSetName.NewRegistrationSync);

            _fixture = new Activity();
            _fixture.NewRegistrationSyncOperation.SerialReconciliationResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.NewRegistrationSyncOperation, _fixture, RuleSetName.NewRegistrationSync);

            _fixture = new Activity();
            _fixture.CoreContactAttributeSyncOperation.SerialCreateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, _fixture, RuleSetName.CoreContactAttributeSync);

            _fixture = new Activity();
            _fixture.CoreContactAttributeSyncOperation.SerialUpdateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, _fixture, RuleSetName.CoreContactAttributeSync);

            _fixture = new Activity();
            _fixture.CoreContactAttributeSyncOperation.SerialReconciliationResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.CoreContactAttributeSyncOperation, _fixture, RuleSetName.CoreContactAttributeSync);

            _fixture = new Activity();
            _fixture.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult1000.FailedBatches.Add(four01To599BulkFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);

            _fixture = new Activity();
            _fixture.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult100.FailedBatches.Add(four01To599BulkFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);

            _fixture = new Activity();
            _fixture.ChildAgeAndGradeSyncOperation.BulkUpdateSyncResult10.FailedBatches.Add(four01To599BulkFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);

            _fixture = new Activity();
            _fixture.ChildAgeAndGradeSyncOperation.SerialCreateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);

            _fixture = new Activity();
            _fixture.ChildAgeAndGradeSyncOperation.RetryBulkUpdateAsSerialUpdateResult.Failures.Add(four01To599SerialFailure);
            _validator.ShouldHaveValidationErrorFor(activity => activity.ChildAgeAndGradeSyncOperation, _fixture, RuleSetName.ChildAgeGradeSync);
        }
    }
}