# Core steps:

1. Setup OpenAI account
   1.1. Create an account
   1.2. Create an API key
   1.3. Top up the balance
2. Implement data access layer
   2.1. Methods to create/update/delete data for all required DB entities
3. Implement business logic layer
   3.1. Implement auth  logic + profile settings (GPT provider, API key (if needed) etc.)
   3.2. Implement OpenAI tests generation
   3.3. Implement test startup logic
   3.4. Implement report generation logic
4. Implement presentation layer
   4.1. Login view
   4.2. User home page view
   4.3. Quiz question view
   4.4. Quiz report view (+ export to .txt file)

# Next steps:

1. Unit tests for business logic layer
2. StyleCop for the whole project
3. Events logging (write to a log file)
4. Animations for the presentation layer

# Finish steps:

1. Prepare for project demo (prepare a presentation, write a script)
