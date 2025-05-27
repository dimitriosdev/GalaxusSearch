/**
 * Dark Mode Test Script
 *
 * This script tests the dark mode functionality by:
 * 1. Checking if the theme toggle button exists
 * 2. Testing both light and dark mode states
 * 3. Verifying that colors change appropriately
 */

export function testDarkMode() {
  console.log("🌗 Testing Dark Mode Functionality...");

  // Test 1: Check if theme toggle exists
  const themeToggle = document.querySelector('[aria-label*="Switch to"]');
  if (!themeToggle) {
    console.error("❌ Theme toggle button not found!");
    return false;
  }
  console.log("✅ Theme toggle button found");

  // Test 2: Test light mode
  document.documentElement.classList.remove("dark");
  const lightBg = getComputedStyle(
    document.querySelector("main")
  ).backgroundColor;
  console.log("🌞 Light mode background:", lightBg);

  // Test 3: Test dark mode
  document.documentElement.classList.add("dark");
  const darkBg = getComputedStyle(
    document.querySelector("main")
  ).backgroundColor;
  console.log("🌙 Dark mode background:", darkBg);

  // Test 4: Verify colors are different
  if (lightBg === darkBg) {
    console.error("❌ Light and dark backgrounds are the same!");
    return false;
  }
  console.log("✅ Light and dark modes have different backgrounds");

  // Test 5: Test input field colors
  const inputField = document.querySelector('input[type="text"]');
  if (inputField) {
    const inputBg = getComputedStyle(inputField).backgroundColor;
    console.log("📝 Input background in dark mode:", inputBg);
  }

  console.log("🎉 Dark mode test completed successfully!");
  return true;
}

// Auto-run the test in development
if (typeof window !== "undefined" && process.env.NODE_ENV === "development") {
  setTimeout(testDarkMode, 1000);
}
