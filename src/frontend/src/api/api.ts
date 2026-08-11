const API_ENDPOINT = process.env.NEXT_PUBLIC_API_ENDPOINT;

export interface ExamData {
    // Exam score (out of 100).
    Score: number,
    // Unique identifier for plagiarism incident.
    IncidentId: string,
    // Unique identifier for exam attempt.
    ExamId: string,
    // Task Token unique to the current Step Functions execution.
    TaskToken: string
}

export async function submitExam(examData: ExamData) {
    const response = await fetch(`${API_ENDPOINT}/exam`, {
        method: 'POST',
        mode: 'cors',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(examData)
    });
    const body = await response.json().catch(() => ({}));
    if (!response.ok) {
        throw new Error(body.message || `Submitting the exam failed with status ${response.status}.`);
    }
    return body;
}

export interface Incident {
    StudentId: string,
    IncidentDate: string
}

export interface StepFunctionInfo {
    executionArn: string
    startDate: string
}

export async function createIncident(incidentData: Incident): Promise<StepFunctionInfo> {
    const response = await fetch(`${API_ENDPOINT}/incident`, { method: 'POST', mode: 'cors', headers: { "Content-Type": "application/json" }, body: JSON.stringify(incidentData) });
    const body = await response.json().catch(() => ({}));
    if (!response.ok) {
        throw new Error(body.message || `Creating the incident failed with status ${response.status}.`);
    }
    const { executionArn, startDate } = body;
    return { executionArn, startDate };
}

