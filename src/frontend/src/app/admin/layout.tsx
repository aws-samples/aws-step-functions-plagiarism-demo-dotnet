import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './custom.scss'


export const metadata: Metadata = {
  title: 'Developing with Step Functions',
  description: '',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  )
}
