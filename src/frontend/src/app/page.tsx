'use client'

import Image from 'next/image'
import styles from './page.module.css'
import Exam from '@/components/Exam'
import { useState } from 'react'

export default function Home() {
  const [examSubmitted, setExamSubmitted] = useState(false)
  const [score, setScore] = useState(0)
  return (
    <main className={styles.main}>
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
              <Exam examSubmitted={false} score={0} setScore={setScore}/>
          </div>
        </div>
    </section>
    </main>
  )
}
