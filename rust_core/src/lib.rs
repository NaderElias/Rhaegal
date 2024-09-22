
use notify_rust::*;

#[no_mangle]
pub extern "C" fn show_notification() {
    Notification::new()
    .summary("Hello World")
    .body("This is a notification from Rust!")
    .show()
    .unwrap(); // Handle errors properly in a real-world application
}
