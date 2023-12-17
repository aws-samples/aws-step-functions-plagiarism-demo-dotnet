type QuestionProps = {
    recordAnswer: (questionId: string, answer: string) => void;
    questionText: string;
    answers: Map<string, string>;
    questionId: string;
    questionNumber: number;
    disabled: boolean;
}

export default function Question({ recordAnswer, questionText, answers, questionId, questionNumber, disabled }: QuestionProps) {

    return (<div>
        <div className="field box question">
            <label className="label">
                <strong> Question {questionNumber + 1}:</strong>
                <em> {questionText}</em>
            </label>
            <div className="control">
                <div className="select is-fullwidth">
                    <select
                        required
                        disabled={disabled}
                        onChange={(e) => { recordAnswer(questionId, e.target.value) }}
                    >
                        <option disabled selected value="">Please select one</option>
                        {Object.entries(answers).map(([optionId, optionText],) => (
                            <option
                                value={optionId}
                                key={optionId}
                            >
                                {optionText}
                            </option>))}
                    </select>
                </div>
            </div>
            <p className="help">Choose the best option.</p>
        </div>

    </div>);
}