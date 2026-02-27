# How To: Monster Behavior (Search / Chase / Attack)

This tutorial builds a classic game AI behavior tree in a way that stays readable in code.

The behavior is simple to describe and very common in action games:

- If a target exists and is in attack range -> **AttackTarget**
- If a target exists but is not in attack range -> **ChaseTarget**
- If no target exists -> **SearchForTarget**

Why this pattern matters: this is exactly the kind of logic that starts as “just 3 if statements” and later becomes hard to reason about when more cases are added. A StateTree keeps that decision logic explicit and structured.

## Tutorial Structure

- [Part 0: Context and Conditions](HowTo.MonsterBehavior.Part0.ContextAndConditions.md)
- [Part 1: Tasks](HowTo.MonsterBehavior.Part1.Tasks.md)
- [Part 2: Build the Tree](HowTo.MonsterBehavior.Part2.Tree.md)

If you only skim one thing, skim Part 2 first: the nested tree setup acts like a visual graph in your IDE.
