import React, { useState } from "react";
import "./App.css";

function App() {
  const [password, setPassword] = useState("");

  const calculatePasswordStrength = (password) => {
    if (password === "") {
      return ["", "", ""]; // All sections are gray
    }

    if (password.length < 8) {
      return ["red", "red", "red"]; // All sections are red
    }

    if (
      /^[a-zA-Z]+$/.test(password) ||
      /^[0-9]+$/.test(password) ||
      /^[!@#$%^&*()_+\-=[\]{};':"|,.<>/?]+$/.test(password)
    ) {
      return ["red", "", ""]; // Easy password
    }

    if (
      /^[a-zA-Z0-9]+$/.test(password) ||
      /^[a-zA-Z!@#$%^&*()_+\-=[\]{};':"|,.<>/?]+$/.test(password) ||
      /^[0-9!@#$%^&*()_+\-=[\]{};':"|,.<>/?]+$/.test(password)
    ) {
      return ["yellow", "yellow", ""]; // Medium password
    }

    return ["green", "green", "green"]; // Strong password
  };

  const handlePasswordChange = (event) => {
    const password = event.target.value;
    setPassword(password);
  };

  const passwordStrength = calculatePasswordStrength(password);

  return (
    <div className="container">
      <h1>Password Strength Checker</h1>
      <input
        type="password"
        id="passwordInput"
        placeholder="Enter password"
        value={password}
        onChange={handlePasswordChange}
      />
      <div className="strength-sections">
        <div className={`strength-section ${passwordStrength[0]}`} />
        <div className={`strength-section ${passwordStrength[1]}`} />
        <div className={`strength-section ${passwordStrength[2]}`} />
      </div>
    </div>
  );
}

export default App;
