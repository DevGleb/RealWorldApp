import React, { useState } from "react";
import "./App.css";

const App = () => {
  const [password, setPassword] = useState("");

  const handleChange = (event) => {
    setPassword(event.target.value);
  };

  const getPasswordStrength = () => {
    if (password.length === 0) {
      return "none";
    } else if (password.length < 8) {
      return "weak";
    } else if (
      /\d/.test(password) &&
      /[a-zA-Z]/.test(password) &&
      /[!@#$%^&*]/.test(password)
    ) {
      return "strong";
    } else {
      return "medium";
    }
  };

  const getStrengthColor = (section) => {
    const strength = getPasswordStrength();
    if (strength === "none") {
      return "gray";
    } else if (strength === "weak") {
      return section === 1 ? "red" : "gray";
    } else if (strength === "medium") {
      return section <= 2 ? "yellow" : "gray";
    } else {
      return "green";
    }
  };

  return (
    <div className="password-card">
      <h1>Password strength check</h1>
      <input
        type="password"
        id="password-input"
        placeholder="Enter password"
        onChange={handleChange}
      />
      <div className="strength-indicator">
        <div className={`strength-section ${getStrengthColor(1)}`}></div>
        <div className={`strength-section ${getStrengthColor(2)}`}></div>
        <div className={`strength-section ${getStrengthColor(3)}`}></div>
      </div>
    </div>
  );
};

export default App;
