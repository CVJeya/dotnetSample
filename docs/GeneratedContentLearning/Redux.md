configureStore()
     ↓
Creates the Redux store 🗄️

createSlice()
     ↓
Creates reducers + actions ⚙️

useSelector()
     ↓
Reads Redux state 👀

useDispatch()
     ↓
Sends actions 📬


import { configureStore } from "@reduxjs/toolkit";
import tasksReducer from "./features/tasks/tasksSlice";
import counterReducer from "./features/counter/counterSlice";
import scoreReducer from "./features/score/scoreSlice";

export const store = configureStore({
  reducer: {
    tasks: tasksReducer,
    counter: counterReducer,
    score: scoreReducer
  }
});

import { createSlice } from "@reduxjs/toolkit";

const tasksSlice = createSlice({
  name: "tasks",

  initialState: [],

 reducers: {
  addTask: (state, action) => {
    state.push(action.payload);
  },

  deleteTask: (state, action) => {
    return state.filter((task) => task.id !== action.payload);
  },

  completeTask: (state, action) => {
  const task = state.find((task) => task.id === action.payload);

  if (task) {
    task.completed = true;
  }
}
}
});
export const { addTask, deleteTask, completeTask } = tasksSlice.actions;
export default tasksSlice.reducer;


import { createSlice } from "@reduxjs/toolkit";

const counterSlice = createSlice({
  name: "counter",

  initialState: 0,

  reducers: {
    increment: (state) => {
      return state + 1;
    }
  }
});

export const { increment } = counterSlice.actions;

export default counterSlice.reducer;



import { useDispatch } from "react-redux"; 
import { addTask } from "./features/tasks/tasksSlice"; 
function AddTask() 
{ 
const dispatch = useDispatch(); 
return ( 
<button onClick={() => dispatch(addTask("Learn Redux"))}> Add Task </button> 
); 
} 
export default AddTask;


import { useSelector,useDispatch  } from "react-redux";
import { deleteTask } from "./features/tasks/tasksSlice";
import { completeTask } from "./features/tasks/tasksSlice";
function TaskList() {
  const tasks = useSelector((state) => state.tasks);
  const dispatch = useDispatch();
  return (
    <ul>
      {tasks.map((task) => (
       <li key={task.id}>{task.completed ? "✅" : "⬜"} {task.text}<button onClick={() => dispatch(completeTask(task.id))}>
  Complete
</button><button onClick={() => dispatch(deleteTask(task.id))}>
  Delete
</button></li>
      ))}
    </ul>
  );
}
export default TaskList;



import { useState } from "react";
import { useDispatch } from "react-redux";
import { addTask } from "./features/tasks/tasksSlice";

function AddTask() {
  const [task, setTask] = useState("");
  const dispatch = useDispatch();

  const handleAdd = () => {
   dispatch(
  addTask({
  id: Date.now(),
  text: task,
  completed: false
})
);
 setTask("");

     };

  return (
    <div>
      <input value={task}  onChange={(event) => setTask(event.target.value)} />
      <button onClick={handleAdd}>Add Task</button>
    </div>
  );
}

export default AddTask;


Counter.jsx

import { useSelector, useDispatch } from "react-redux";
import { increment } from "./counterSlice";

function Counter() {
  const count = useSelector((state) => state.counter);
  const dispatch = useDispatch();

  return (
    <div>
      <p>{count}</p>
      <button onClick={() => dispatch(increment())}>
        Increment
      </button>
    </div>
  );
}

export default Counter;

🧠 Remember

useSelector() means “Give me something from the Redux store.”

🧠 Great! You now understand the core Redux Toolkit flow:
Component
   ↓
dispatch(action + payload)
   ↓
Reducer
   ↓
action.payload
   ↓
Redux Store
   ↓
useSelector()
   ↓
Component updates



Score.jsx

import { createSlice } from "@reduxjs/toolkit";

const scoreSlice = createSlice({
  name: "score",

  initialState: {
    score: 0,
    lastReason: ""
  },

  reducers: {
    addScore: (state, action) => {
      state.score += action.payload.points;
      state.lastReason = action.payload.reason;
    }
  }
});

export const { addScore } = scoreSlice.actions;
export default scoreSlice.reducer;

Updated Score.jsx
import { useSelector, useDispatch } from "react-redux";
import { addScore } from "./scoreSlice";

function Score() {
  const currentScore = useSelector((state) => state.score.score);
  const lastReason = useSelector((state) => state.score.lastReason);
  const dispatch = useDispatch();


  return (
    <div>
      <p>{currentScore}</p>
<p>Reason: {lastReason}</p>
      <button onClick={() => dispatch(addScore({
  points: 5,
  reason: "Bonus!"
})))}>
        Add 5
      </button>
    </div>
  );
}

export default Score;