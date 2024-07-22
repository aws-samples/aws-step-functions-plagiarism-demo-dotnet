// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;

namespace Plagiarism;

public class IncidentNotFoundException : Exception
{
    public IncidentNotFoundException()
    {
    }

    public IncidentNotFoundException(string message) : base((string)message)
    {
    }
}