# Walnut Ai Slash Commands Learning Guide 🚀

**Learn all 10 slash commands step by step!**

---

## Table of Contents
1. [What Are Slash Commands?](#what-are-slash-commands)
2. [The 10 Slash Commands](#the-10-slash-commands)
3. [How to Use Them](#how-to-use-them)
4. [Real Examples](#real-examples)
5. [Practice Exercises](#practice-exercises)
6. [Cheat Sheet](#cheat-sheet)

---

## What Are Slash Commands?

Slash commands are **shortcuts** that tell Walnut Ai to do special things. Instead of typing a long sentence, you type `/command-name` and Walnut does the job.

### Why Use Them?
- ⚡ **Faster** — Type less, do more
- 🎯 **Focused** — Tell Walnut exactly what to do
- 🧠 **Smarter** — Walnut remembers what you're doing
- 🔧 **Powerful** — Control the AI like a pro

---

## The 10 Slash Commands

### Command #1: `/clear` 🧹

**What it does:** Deletes your ENTIRE conversation history

**Symbol meaning:** 🧹 = Cleaning/Erasing

**When to use:**
- Starting a brand NEW project
- You want a fresh start with no old code
- Too much clutter in the chat
- Starting a completely different topic

**How to type it:**
```
/clear
```

**What happens:**
```
Before:  [Old chat with 50 messages]
         [Old code, old errors, old conversations]

After:   [EMPTY - Everything gone!]
```

**⚠️ WARNING:** 
- This CANNOT be undone!
- Once you `/clear`, all old messages disappear forever
- Only use if you're 100% sure!

**When NOT to use:**
- ❌ If you still need old code
- ❌ If you might want to look back
- ❌ If you're unsure

**Good Use Case:**
```
You: "I finished Project A. Now starting Project B."
You: /clear
Why? Project B has nothing to do with Project A
```

---

### Command #2: `/compact` 🗜️

**What it does:** Cleans up old messages, keeps important ones

**Symbol meaning:** 🗜️ = Squeezing/Compressing

**When to use:**
- You have a REALLY LONG conversation (100+ messages)
- The chat is getting slow
- You want to keep important info but remove the rest
- Walnut is confused because of too much old info

**How to type it:**
```
/compact
```

**What happens:**
```
Before:  Message 1: "Hello"
         Message 2: "Write code"
         Message 3: "Fix the code"
         Message 4: "Good job"
         ... (100 more messages)

After:   [Summary of important parts]
         [Keeps the key code and solutions]
         [Removes the small talk]
```

**Difference from `/clear`:**
- `/clear` = Erase EVERYTHING (can't get back)
- `/compact` = Keep important stuff, erase the rest (smart)

**Good Use Case:**
```
You've been chatting for 2 hours
Chat has: Bad attempts, fixes, more fixes, questions, answers
You: /compact
Why? Walnut keeps the GOOD solutions, forgets the mistakes
```

---

### Command #3: `/init` 🚀

**What it does:** Sets up a NEW workspace or project

**Symbol meaning:** 🚀 = Starting/Launching

**When to use:**
- Creating a NEW project from scratch
- Setting up a NEW coding workspace
- You need to initialize (start) something
- Starting custom methods or tools

**How to type it:**
```
/init
```

**What happens:**
```
Before:  [Empty folder]

After:   [Folders created]
         [Config files made]
         [Everything setup for you]
```

**Example Setup:**
```
/init might create:
├── src/
├── config.json
├── package.json
└── README.md
```

**Good Use Case:**
```
You: "I want to start a new website"
You: /init
Why? Creates all folders and files you need to start
```

---

### Command #4: `/context` 🧠

**What it does:** Shows/gives Walnut info about your project

**Symbol meaning:** 🧠 = Brain/Memory

**When to use:**
- You want Walnut to KNOW about your project
- You have files you want Walnut to understand
- You want to teach Walnut about your code structure
- You want Walnut to remember important background info

**How to type it:**
```
/context
```

**What happens:**
```
You type: /context

Walnut shows:
- Files in your project
- Folder structure
- Important information
- What Walnut knows about your code
```

**Why it's important:**
Walnut is like a helper. If you don't tell it about your project, it can't help properly!

```
Bad:  You: "Fix my error"
      Walnut: "I don't know what files you have..."

Good: You: /context
      Walnut: "I see your project structure!"
      You: "Fix my error"
      Walnut: "Fixed! Here it is..."
```

**Good Use Case:**
```
You have: website/index.html, website/style.css, website/script.js
You: /context
Why? Walnut knows you have a website project, can help better
```

---

### Command #5: `/undo` ↩️

**What it does:** Takes back the last thing Walnut did

**Symbol meaning:** ↩️ = Going backwards

**When to use:**
- Walnut made a mistake
- You don't like Walnut's answer
- You changed your mind
- You want to try a different approach

**How to type it:**
```
/undo
```

**What happens:**
```
Step 1: You: "Write a game"
Step 2: Walnut: [Writes 100 lines of code]
Step 3: You: "/undo"
Step 4: Walnut: [Code disappears, goes back to Step 1]
Step 5: You: "Write a game differently"
```

**Why use it:**
If you don't like what Walnut did, go back and try again!

**Good Use Case:**
```
You: "Make this code simpler"
Walnut: [Simplifies it]
You: "Hmm, this is more confusing"
You: /undo
Why? Go back and try a different simplification
```

---

### Command #6: `/retry` 🔄

**What it does:** Tries the last thing again, but with a DIFFERENT approach

**Symbol meaning:** 🔄 = Repeating/Trying again

**When to use:**
- The first answer didn't work
- You want a different solution
- The code Walnut wrote has a bug
- You want Walnut to try another way

**How to type it:**
```
/retry
```

**What happens:**
```
Attempt 1: Walnut tries one way
Result:    Doesn't work or you don't like it

You type:  /retry

Attempt 2: Walnut tries a DIFFERENT way
Result:    Hopefully better!
```

**Difference from `/undo`:**
- `/undo` = Go back to before (erase what happened)
- `/retry` = Try again differently (keep trying)

**Good Use Case:**
```
You: "Write a simple calculator"
Walnut: [Writes one way - complicated]
You: "That's too complicated"
You: /retry
Why? Walnut writes a simpler calculator version
```

---

### Command #7: `/help` ❓

**What it does:** Shows you help information

**Symbol meaning:** ❓ = Asking for help

**When to use:**
- You're confused about how to use Walnut
- You forgot what a command does
- You need guidance or instructions
- You want to learn about features

**How to type it:**
```
/help
```

**What happens:**
```
You type: /help

Walnut shows:
- How to use commands
- Tips and tricks
- Instructions for different features
- Examples of what you can do
```

**When to use:**
- "I don't understand what to do"
- "How do I use this slash command?"
- "What can Walnut do for me?"
- "I forgot how to..."

**Good Use Case:**
```
You: /help
Why? See all the features and instructions
```

---

### Command #8: `/skill <install|list|remove>` 🛠️

**What it does:** Manage special tools/skills (advanced features)

**Symbol meaning:** 🛠️ = Tools/Building

**The 3 options:**

#### **Option A: `/skill list`** 📋
Shows all skills you can use

```
/skill list

Result:
- Skill 1: PDF reading
- Skill 2: Code analysis
- Skill 3: Database queries
... (all available skills)
```

**When to use:**
- You want to see what skills are available
- You want to know what tools Walnut has
- You're exploring what you can do

#### **Option B: `/skill install <skill-name>`** ⬇️
Adds a new skill/tool

```
/skill install pdf-analyzer

Result:
- Downloads the skill
- Sets it up
- Now you can use it!
```

**When to use:**
- You found a skill you want to use
- You need a specific tool
- You want to add new features to Walnut

#### **Option C: `/skill remove <skill-name>`** 🗑️
Removes/deletes a skill

```
/skill remove pdf-analyzer

Result:
- Skill is deleted
- You can't use it anymore
- Saves space
```

**When to use:**
- You don't need a skill anymore
- You want to clean up
- You have too many installed

**Good Use Case:**
```
You: /skill list
Result: See all skills available

You: "I want to read PDFs"
You: /skill install pdf-reader
Result: Now you can read PDF files!

You: /skill list
Result: pdf-reader is now installed ✓
```

---

### Command #9: `/loop <interval> <prompt>` 🔁

**What it does:** Repeats an action over and over at regular times

**Symbol meaning:** 🔁 = Going in circles/Repeating

**Time options you can use:**
- `5s` = every 5 seconds
- `30s` = every 30 seconds
- `1m` = every 1 minute
- `5m` = every 5 minutes
- `10m` = every 10 minutes
- `1h` = every 1 hour

**How to type it:**
```
/loop <time> "<what to do>"

Examples:
/loop 5s "Check if server is running"
/loop 1m "Run all tests"
/loop 10m "Update database"
```

**What happens:**
```
You type: /loop 5s "Check server"

Result:
First time:   Check server ✓
5 seconds later: Check server ✓
5 seconds later: Check server ✓
5 seconds later: Check server ✓
(keeps repeating until you stop it)
```

**When to use:**
- Testing something repeatedly
- Checking if something is working
- Running automated checks
- Monitoring a system
- Automation tasks

**Good Use Case:**
```
You: /loop 10s "Test if my game runs"
Result:
- Runs test
- Waits 10 seconds
- Runs test again
- Keeps checking every 10 seconds
Why? See if your game keeps working over time
```

**How to STOP a loop:**
```
Press: Ctrl+C (or Cmd+C on Mac)
Or: Click the stop button
```

---

### Command #10: `/goal <condition>` 🎯

**What it does:** Sets a TARGET for Walnut to work toward

**Symbol meaning:** 🎯 = Aiming at a target

**When to use:**
- You have a specific end goal
- You want Walnut to keep trying until success
- Testing or debugging something
- You want Walnut focused on ONE thing

**How to type it:**
```
/goal "<what you want>"

Examples:
/goal "Make all tests pass"
/goal "Fix the login bug"
/goal "Get 100% code coverage"
/goal "Make the website load in 2 seconds"
```

**What happens:**
```
You type: /goal "Make code run without errors"

Walnut thinks:
- Goal is clear: NO errors
- I'll keep trying until goal is met
- When code runs with 0 errors = SUCCESS
- Goal achieved! ✓
```

**Why use it:**
Walnut stays focused on YOUR goal, not distracted

```
Without /goal:
You: "Write code"
Walnut: [Writes something]
You: "That's not what I wanted..."

With /goal:
You: /goal "Write code that adds two numbers"
Walnut: [Focused on THIS goal]
Result: Code adds two numbers ✓
```

**Good Use Case:**
```
You: /goal "Fix all spelling errors in my document"
Walnut: Finds and fixes every spelling mistake
Result: Goal achieved! ✓
```

---

## How to Use Them

### Rule #1: One Command at a Time ⏱️
```
Good:   /clear
        (wait for it to finish)
        /init

Bad:    /clear
        /init
        /context
        (all at once - confusing!)
```

### Rule #2: Read the Result Before Next Step 👀
```
Step 1: Type /context
Step 2: READ what it shows
Step 3: THEN type your next command
```

### Rule #3: Use `/help` When Confused ❓
```
You don't understand something?
Type: /help
```

### Rule #4: Combine With Regular Talking 💬
```
Step 1: /context (show your project)
Step 2: "Walnut, help me debug this error"
Step 3: Walnut helps!
```

---

## Real Examples

### Example 1: Learning to Code 📚

**Goal:** Learn how to write a simple calculator

```
Step 1: /clear
        Why? Fresh start, no distractions

Step 2: /init
        Why? Set up a new project

Step 3: /context
        Why? Show Walnut what you have

Step 4: "Teach me how to write a calculator, step by step"
        Why? Ask to learn

Step 5: /goal "Understand how calculators work"
        Why? Focus on learning goal

Step 6: "Explain the code you wrote"
        Why? Understand the parts

Step 7: /retry
        (if you didn't understand - try different explanation)

Result: ✓ You learned how calculators work!
```

---

### Example 2: Debugging Code 🐛

**Goal:** Fix a broken program

```
Step 1: /context
        Why? Show Walnut your code files

Step 2: "My program crashes when I click the button"
        Why? Describe the problem

Step 3: /goal "Fix the crash, make button work"
        Why? Focus on fixing goal

Step 4: Walnut: "Found the bug! Here's the fix..."
        
Step 5: /retry
        (if first fix doesn't work)

Step 6: /loop 5s "Test if button works"
        Why? Check every 5 seconds

Result: ✓ Button works! Bug fixed!
```

---

### Example 3: Automation Task ⚙️

**Goal:** Run tests automatically

```
Step 1: /skill list
        Why? See what tools are available

Step 2: /skill install test-runner
        Why? Get a tool to run tests

Step 3: /goal "Run all tests and pass"
        Why? Set the goal

Step 4: /loop 1m "Run all tests"
        Why? Run tests every minute automatically

Step 5: Wait and watch tests run automatically ✓

Result: ✓ Tests run automatically every minute!
```

---

### Example 4: Long Project Work 🏗️

**Goal:** Work on a project for several hours

```
Hour 1:
Step 1: /init
Step 2: /context
Step 3: Start building...

Hour 2:
Step 1: Lots of messages, chat is slow
Step 2: /compact
        Why? Clean up, keep important stuff

Hour 3:
Step 1: Get confused
Step 2: /help
        Why? Remember what you can do

Hour 4:
Step 1: Finished!
Step 2: /clear
        Why? Ready for next project
```

---

## Practice Exercises

### Exercise 1: Learn Slash Commands Yourself ⭐

**What to do:**
1. Type: `/help`
2. Read what it tells you
3. Try `/context`
4. See what happens
5. Try `/skill list`
6. See what skills are available

**Time:** 5 minutes

---

### Exercise 2: Set Up a New Project 🚀

**What to do:**
1. Type: `/clear`
2. Type: `/init`
3. Type: `/context`
4. Tell Walnut: "I want to build a simple game"
5. See how Walnut helps!

**Time:** 10 minutes

---

### Exercise 3: Learn Something New 📚

**What to do:**
1. Type: `/goal "Learn how loops work"`
2. Type: "Teach me about loops, simply"
3. Read Walnut's explanation
4. Type: `/retry` (if you didn't understand)
5. Ask: "Can you give me an example?"

**Time:** 15 minutes

---

### Exercise 4: Practice `/loop` 🔁

**What to do:**
1. Type: `/loop 3s "Say hello"`
2. Watch Walnut say hello every 3 seconds
3. After 6-7 times, press Ctrl+C to stop
4. Try: `/loop 1m "Ask me a question"`
5. Stop after 3 questions

**Time:** 5 minutes

---

### Exercise 5: Set a Goal and Achieve It 🎯

**What to do:**
1. Type: `/goal "Write a program that counts to 10"`
2. Say: "Help me write a program that counts to 10"
3. Walnut writes it for you
4. Type: `/goal "Make it count to 100"`
5. Say: "Now make it count to 100"
6. Goal achieved! ✓

**Time:** 10 minutes

---

## Cheat Sheet

### Quick Reference Table 📋

| # | Command | Does What | When to Use |
|---|---------|-----------|------------|
| 1 | `/clear` | Erase everything | Fresh start |
| 2 | `/compact` | Clean up old messages | Long chats |
| 3 | `/init` | Set up new project | Starting out |
| 4 | `/context` | Show project info | Give Walnut context |
| 5 | `/undo` | Go back one step | Undo mistakes |
| 6 | `/retry` | Try again differently | When stuck |
| 7 | `/help` | Show instructions | Need guidance |
| 8 | `/skill` | Manage tools | Install/remove skills |
| 9 | `/loop` | Repeat action | Automation/testing |
| 10 | `/goal` | Set target | Focus on goal |

---

### Memory Aid (How to Remember) 🧠

**Think of a video game:**

```
/clear    = New game, delete save
/compact  = Delete old levels, keep progress
/init     = Start new game, create character
/context  = Show game stats and inventory
/undo     = Undo last move
/retry    = Try level again differently
/help     = Show game instructions
/skill    = Buy new weapons/tools
/loop     = Repeat training exercise
/goal     = Set quest objective
```

---

### When to Use Each Command 🎯

**For Learning:**
- `/context` → `/help` → `/goal` → `/retry`

**For Debugging:**
- `/context` → `/goal` → `/retry` → `/loop`

**For Projects:**
- `/clear` → `/init` → `/context` → `/compact`

**For Tools:**
- `/skill list` → `/skill install` → `/skill remove`

**For Long Work:**
- `/compact` → `/goal` → `/retry`

---

## Tips & Tricks 💡

### Tip #1: Read Output Before Next Step
```
Don't rush!
Type /context → READ → Then next command
```

### Tip #2: Use /help When Lost
```
Confused? Type /help
It always helps!
```

### Tip #3: Combine Commands
```
Good workflow:
/context → /goal → /retry → /loop
```

### Tip #4: Save Important Things
```
Before /clear, make sure you:
- Copied important code
- Saved your work
- Don't need old messages
```

### Tip #5: /goal Keeps Walnut Focused
```
Instead of vague requests:
Bad:  "Write something"
Good: /goal "Write a hello world program"
```

### Tip #6: /loop Saves Time
```
Testing things?
/loop 5s "Run test"
(runs automatically, you don't have to!)
```

### Tip #7: /retry Tries Different Ways
```
If Walnut's answer is wrong:
/retry (tries differently!)
/retry again (tries even more differently!)
```

---

## Common Questions 🤔

### Q: Can I undo `/clear`?
**A:** No! `/clear` is permanent. Be careful!

### Q: What's the difference between `/clear` and `/compact`?
**A:** `/clear` = Delete everything. `/compact` = Keep important, delete rest.

### Q: Can I stop `/loop`?
**A:** Yes! Press Ctrl+C (or Cmd+C on Mac)

### Q: What does `/skill install` do?
**A:** Downloads and sets up a new tool you can use.

### Q: How many times will `/loop` repeat?
**A:** Until you stop it with Ctrl+C. It repeats forever!

### Q: Can I change a `/goal` once I set it?
**A:** Yes! Just type a new `/goal` to change it.

### Q: Should I use `/context` every time?
**A:** Good idea! It helps Walnut understand your project.

### Q: Is `/undo` the same as `/retry`?
**A:** No! `/undo` goes back. `/retry` tries again differently.

---

## Your Learning Path 🚀

### Day 1: Learn the Basics
- Read this document
- Try `/help`
- Try `/context`
- Try `/skill list`
- **Time:** 30 minutes

### Day 2: Practice Each Command
- Try `/init`
- Try `/undo`
- Try `/retry`
- Try `/goal`
- **Time:** 1 hour

### Day 3: Real Projects
- Use `/context` + `/goal` + `/retry`
- Try `/loop` for automation
- Use `/compact` on long chats
- **Time:** 1-2 hours

### Day 4: Advanced Usage
- Combine commands together
- Use `/skill install` for new tools
- Build real projects with slash commands
- **Time:** As much as you want!

---

## Remember! 🌟

1. **Start simple** — Learn one command at a time
2. **Read the output** — Always see what Walnut does
3. **Ask for help** — Type `/help` anytime
4. **Practice** — Try the exercises
5. **Have fun** — Walnut is here to help you!

You've got this! 💪

---

**Happy learning!** 🎉

Created for Walnut Ai Users • All Ages • All Skill Levels
