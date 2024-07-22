// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

using System;

namespace Plagiarism;

public class ExamNotFoundException : Exception
{
    public ExamNotFoundException()
    {
    }

    public ExamNotFoundException(string message) : base((string)message)
    {
    }
}