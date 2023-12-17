'use client'

import Exam from '@/app/testcentre/components/Exam'
import { useState } from 'react'
import ExamIntegration from './components/ExamIntegration'


export default function ExamPage() {
  const [examSubmitted, setExamSubmitted] = useState(false)
  const [score, setScore] = useState(0)
  return (
    <main>
      <section id="exam-hero" className="hero is-dark">
        <div className="hero-body">
          <h1 className="title">AWS Step Functions Plagiarism Exam</h1>
          <p className="subtitle">
            Please re-attempt the exam. Once you are done, submit your exam for marking.
          </p>
        </div>
      </section>
      <section className="section">
        <div className="container">
          <div id="app">
              <Exam examSubmitted={examSubmitted} score={score} setScore={setScore}/>
              <ExamIntegration score={score} setExamSubmitted={setExamSubmitted}/>
          </div>
        </div>
    </section>
    </main>
  )
}
