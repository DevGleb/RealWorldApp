export const getCurrentDate = () => {
  const currentData = new Date().toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });
  return currentData;
};
