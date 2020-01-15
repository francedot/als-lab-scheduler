# Azure Lab Services - Lab Scheduler

This is currently a proof of concept demonstrating how to schedule classes for a lab starting from a iCalendar (.ics) file.

#### Notes
- As of now, recurrent events are splitted into one-time-only events.
- We need to add the logic for specifying recurrence patterns (daily, weekly..) based on the parameters supported by Lab Services APIs.
- Also need to support no-end events. For now I am limiting getting occurrences up to 20 years.
