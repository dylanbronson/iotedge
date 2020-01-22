// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.ModuleUtil.TestResults
{
    using System;
    using System.Net;
    using Microsoft.Azure.Devices.Edge.Util;
    using Newtonsoft.Json;

    public class DirectMethodTestResult : TestResultBase
    {
        public DirectMethodTestResult(
            string source,
            DateTime createdAt,
            string trackingId,
            Guid batchId,
            string sequenceNumber,
            HttpStatusCode result)
            : base(source, TestOperationResultType.DirectMethod, createdAt)
        {
            this.TrackingId = trackingId ?? string.Empty;
            this.BatchId = Preconditions.CheckNotNull(batchId, nameof(batchId)).ToString();
            this.SequenceNumber = Preconditions.CheckNonWhiteSpace(sequenceNumber, nameof(sequenceNumber));
            this.Result = Preconditions.CheckNotNull(result, nameof(result));
        }

        public string TrackingId { get; set; }

        public string BatchId { get; set; }

        public string SequenceNumber { get; set; }

        public HttpStatusCode Result { get; set; }

        public override string GetFormattedResult()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
