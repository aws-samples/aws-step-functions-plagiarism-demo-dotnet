import type { Metadata } from 'next'
import './globals.scss'

export const metadata: Metadata = {
  title: 'AWS Step Functions Plagiarism Demo - Test Centre',
  description: '',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
       <link rel="icon" href="/images/favicon.ico"/>
      <body>{children}</body>
    </html>
  )
}
