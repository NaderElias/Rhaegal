#[no_mangle]
pub extern "C" fn show_notification() {
    use std::process::Command;
    
    // Using PowerShell to create a notification
    Command::new("powershell")
        .arg("-Command")
        .arg("New-BurntToastNotification -Text 'Hello World!'")
        .output()
        .expect("Failed to execute PowerShell command");
}
