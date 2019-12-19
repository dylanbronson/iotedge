// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Azure.Devices.Edge.Util.Concurrency;

namespace TestResultCoordinator.storage
{
    class StoragePreparer
    {
        AtomicBoolean NetworkStatus = new AtomicBoolean(true);
        static TestOperationResult PrepareTestOperationResult(TestOperationResult testOperationResult)
        {
            switch (testOperationResult.Type)
            {
                case "DirectMethod":
                    // Add to testOperationResult Network Status
                    break;
                case "Network":
                    // Set the atomic boolean to network status
                    break;
                    
            }
        }
    }
}
