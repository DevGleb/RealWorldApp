"use client";
import SearchIcon from "@mui/icons-material/Search";

interface InputProps {
  handleSearch: (event: React.KeyboardEvent<HTMLInputElement>) => void;
  setLocation: React.Dispatch<React.SetStateAction<string>>;
}

const Input = ({ handleSearch, setLocation }: InputProps) => {
  return (
    <form className="flex items-center md:w-2/4 w-full order-2 md:order-1">
      <input
        type="text"
        className="w-full py-2 px-4 rounded-lg border-2 border-gray-300 focus:outline-none focus:border-blue-500 text-black placeholder-gray-400"
        placeholder="Search..."
        onKeyDown={handleSearch}
        onChange={(e) => setLocation(e.target.value)}
      />
    </form>
  );
};

export default Input;
