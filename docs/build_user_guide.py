from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.platypus import (
    Image,
    KeepTogether,
    ListFlowable,
    ListItem,
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "output" / "pdf" / "CatX-User-Guide-v1.0.2.pdf"
LOGO = ROOT / "src" / "CatX" / "Assets" / "DigitalRCS_Logo.png"
CAT_ICON = ROOT / "src" / "CatX" / "Assets" / "catx-app-icon.png"

NAVY = colors.HexColor("#102B55")
INK = colors.HexColor("#253238")
TEAL = colors.HexColor("#008F95")
CORAL = colors.HexColor("#F46F55")
CREAM = colors.HexColor("#FFF8E8")
PALE_TEAL = colors.HexColor("#E5F5F3")
MUTED = colors.HexColor("#617076")


def fit_image(path: Path, width: float, height: float) -> Image:
    image = Image(str(path))
    scale = min(width / image.imageWidth, height / image.imageHeight)
    image.drawWidth = image.imageWidth * scale
    image.drawHeight = image.imageHeight * scale
    return image


def bullet_list(items, style):
    return ListFlowable(
        [ListItem(Paragraph(item, style), leftIndent=12, spaceAfter=4) for item in items],
        bulletType="bullet",
        start="-",
        leftIndent=20,
        bulletFontName="Helvetica",
        bulletFontSize=9,
        bulletColor=TEAL,
        spaceAfter=10,
    )


def numbered_list(items, style):
    return ListFlowable(
        [ListItem(Paragraph(item, style), leftIndent=12) for item in items],
        bulletType="1",
        leftIndent=22,
        bulletFontName="Helvetica-Bold",
        bulletColor=NAVY,
        spaceAfter=8,
    )


def callout(text, style, background=PALE_TEAL, border=TEAL):
    table = Table([[Paragraph(text, style)]], colWidths=[6.55 * inch])
    table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, -1), background),
                ("BOX", (0, 0), (-1, -1), 1, border),
                ("LEFTPADDING", (0, 0), (-1, -1), 12),
                ("RIGHTPADDING", (0, 0), (-1, -1), 12),
                ("TOPPADDING", (0, 0), (-1, -1), 10),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 10),
            ]
        )
    )
    return table


def later_page(canvas, document):
    canvas.saveState()
    width, height = letter
    canvas.setStrokeColor(colors.HexColor("#D6E3E5"))
    canvas.line(0.65 * inch, height - 0.55 * inch, width - 0.65 * inch, height - 0.55 * inch)
    canvas.setFont("Helvetica-Bold", 9)
    canvas.setFillColor(NAVY)
    canvas.drawString(0.65 * inch, height - 0.42 * inch, "CATX KEYBOARD GUARD")
    canvas.setFont("Helvetica", 8)
    canvas.setFillColor(MUTED)
    canvas.drawRightString(width - 0.65 * inch, height - 0.42 * inch, "Dan Roberts - DigitalRCS")
    canvas.line(0.65 * inch, 0.52 * inch, width - 0.65 * inch, 0.52 * inch)
    canvas.drawString(0.65 * inch, 0.33 * inch, "Runs locally - No account - No data collection")
    canvas.drawRightString(width - 0.65 * inch, 0.33 * inch, f"Page {document.page}")
    canvas.restoreState()


def first_page(canvas, document):
    canvas.saveState()
    width, _ = letter
    canvas.setFillColor(NAVY)
    canvas.rect(0, 0, width, 0.28 * inch, fill=1, stroke=0)
    canvas.restoreState()


def build_pdf():
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    document = SimpleDocTemplate(
        str(OUTPUT),
        pagesize=letter,
        rightMargin=0.72 * inch,
        leftMargin=0.72 * inch,
        topMargin=0.72 * inch,
        bottomMargin=0.68 * inch,
        title="CatX Keyboard Guard User Guide",
        author="Dan Roberts - DigitalRCS",
        subject="Installation, configuration, safety, troubleshooting, privacy, and uninstall guidance for CatX.",
    )

    base = getSampleStyleSheet()
    title = ParagraphStyle(
        "CoverTitle",
        parent=base["Title"],
        fontName="Helvetica-Bold",
        fontSize=30,
        leading=34,
        textColor=NAVY,
        alignment=TA_CENTER,
        spaceAfter=10,
    )
    subtitle = ParagraphStyle(
        "CoverSubtitle",
        parent=base["Normal"],
        fontName="Helvetica",
        fontSize=13,
        leading=18,
        textColor=MUTED,
        alignment=TA_CENTER,
    )
    h1 = ParagraphStyle(
        "Heading1Brand",
        parent=base["Heading1"],
        fontName="Helvetica-Bold",
        fontSize=20,
        leading=24,
        textColor=NAVY,
        spaceBefore=4,
        spaceAfter=10,
    )
    h2 = ParagraphStyle(
        "Heading2Brand",
        parent=base["Heading2"],
        fontName="Helvetica-Bold",
        fontSize=13,
        leading=16,
        textColor=TEAL,
        spaceBefore=9,
        spaceAfter=5,
    )
    body = ParagraphStyle(
        "BodyBrand",
        parent=base["BodyText"],
        fontName="Helvetica",
        fontSize=9.5,
        leading=13.5,
        textColor=INK,
        spaceAfter=7,
    )
    small = ParagraphStyle(
        "SmallBrand",
        parent=body,
        fontSize=8.4,
        leading=11.5,
    )
    table_header = ParagraphStyle(
        "TableHeaderBrand",
        parent=small,
        fontName="Helvetica-Bold",
        textColor=colors.white,
    )
    centered = ParagraphStyle("CenteredBrand", parent=body, alignment=TA_CENTER)
    code = ParagraphStyle(
        "CodeBrand",
        parent=body,
        fontName="Courier",
        fontSize=8.5,
        leading=11,
        backColor=colors.HexColor("#F2F5F5"),
        borderColor=colors.HexColor("#D6E3E5"),
        borderWidth=0.5,
        borderPadding=6,
    )

    story = []

    logo = fit_image(LOGO, 4.4 * inch, 2.35 * inch)
    logo.hAlign = "CENTER"
    icon = fit_image(CAT_ICON, 1.35 * inch, 1.35 * inch)
    icon.hAlign = "CENTER"
    story.extend(
        [
            Spacer(1, 0.18 * inch),
            logo,
            Spacer(1, 0.22 * inch),
            icon,
            Spacer(1, 0.18 * inch),
            Paragraph("CatX Keyboard Guard", title),
            Paragraph("User Guide - Version 1.0.2", subtitle),
            Spacer(1, 0.22 * inch),
            callout(
                "A friendly Windows 11 keyboard guard for the moments when your cat decides the keyboard is the best seat in the house.",
                centered,
                CREAM,
                CORAL,
            ),
            Spacer(1, 0.28 * inch),
            Paragraph("Created by <b>Dan Roberts - DigitalRCS</b>", centered),
            Paragraph("Runs locally - No account - No network requests - No data collection", centered),
            PageBreak(),
        ]
    )

    story.extend(
        [
            Paragraph("Install and get started", h1),
            Paragraph(
                "CatX temporarily ignores ordinary keyboard input while keeping the mouse available and displaying an animated desktop cat.",
                body,
            ),
            Paragraph("Install CatX", h2),
            numbered_list(
                [
                    "Download the current <b>CatX-Setup</b> installer from the official DigitalRCS CatX release.",
                    "Double-click the installer and review the license and installation location.",
                    "Optionally select <b>Create a desktop shortcut</b>.",
                    "Choose <b>Install</b>, then <b>Launch CatX</b>.",
                ],
                body,
            ),
            callout(
                "<b>Source check:</b> If Windows SmartScreen identifies an unsigned build, verify that it came from the official CatX release before choosing <b>More info</b> and <b>Run anyway</b>. Never bypass a warning for a file from an unknown source.",
                small,
                colors.HexColor("#FFF2D8"),
                colors.HexColor("#D89B24"),
            ),
            Spacer(1, 0.08 * inch),
            Paragraph("Quick start", h2),
            numbered_list(
                [
                    "Choose a desktop cat or <b>No cat</b>.",
                    "Choose and remember a private recovery shortcut.",
                    "Select <b>Enable keyboard guard</b>.",
                    "Test the recovery shortcut immediately.",
                    "Use the shortcut or <b>Disable guard with mouse</b> to restore typing.",
                ],
                body,
            ),
            Paragraph("System requirements", h2),
            bullet_list(
                [
                    "64-bit Windows 11 PC.",
                    "A mouse or touchpad for the always-available mouse fallback.",
                    "No .NET installation is required; CatX is packaged as a self-contained application.",
                    "No administrator privileges, account, or Internet connection are required to run CatX.",
                ],
                body,
            ),
            PageBreak(),
        ]
    )

    settings_data = [
        [Paragraph("Setting", table_header), Paragraph("What it controls", table_header)],
        [Paragraph("Desktop cat", small), Paragraph("No cat; or one of five original animated vector cats: Marmalade, Midnight, Snowball, Tuxedo, and Calico.", small)],
        [Paragraph("Unlock combination", small), Paragraph("The private key combination that restores typing while the guard is active.", small)],
        [Paragraph("Cat moves every", small), Paragraph("How frequently the animated cat chooses a new desktop location. Disabled when No cat is selected.", small)],
        [Paragraph("Auto-lock after no activity", small), Paragraph("Optional inactivity period. Keyboard or mouse activity resets the timer; the guard enables only after the full period with no input.", small)],
    ]
    settings_table = Table(settings_data, colWidths=[1.5 * inch, 5.0 * inch], repeatRows=1)
    settings_table.setStyle(
        TableStyle(
            [
                ("BACKGROUND", (0, 0), (-1, 0), NAVY),
                ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
                ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#C8D9DB")),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 7),
                ("RIGHTPADDING", (0, 0), (-1, -1), 7),
                ("TOPPADDING", (0, 0), (-1, -1), 6),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
                ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F5F9F9")]),
            ]
        )
    )
    story.extend(
        [
            Paragraph("Configure and use CatX", h1),
            settings_table,
            Paragraph("Recovery shortcut choices", h2),
            bullet_list(
                [
                    "<b>Ctrl + Alt + K</b>",
                    "<b>Ctrl + Shift + F12</b>",
                    "<b>Alt + Shift + Pause</b>",
                ],
                body,
            ),
            callout(
                "<b>Always test the selected recovery shortcut immediately after enabling the guard.</b> The mouse remains usable even when keyboard input is ignored.",
                body,
            ),
            Paragraph("What happens while the guard is active", h2),
            bullet_list(
                [
                    "Ordinary keyboard input is ignored.",
                    "The mouse remains available.",
                    "Minimizing CatX moves it to the Windows notification area. Right-click the cat icon to reopen CatX, disable an active keyboard guard, or exit.",
                    "The selected cat moves across the combined Windows desktop and always faces its travel direction.",
                    "The vector cats use smooth eased movement transitions.",
                    "Closing CatX removes its keyboard hook and restores normal input.",
                ],
                body,
            ),
            PageBreak(),
        ]
    )

    story.extend(
        [
            Paragraph("Safety, recovery, and troubleshooting", h1),
            callout(
                "Windows handles <b>Ctrl + Alt + Delete</b> outside CatX. The secure Windows screen remains available even while the guard is active.",
                body,
                colors.HexColor("#E9F2FF"),
                colors.HexColor("#3178C6"),
            ),
            Paragraph("If typing does not return", h2),
            numbered_list(
                [
                    "Click <b>Disable guard with mouse</b> in the CatX window.",
                    "Close the CatX window with the mouse.",
                    "Press <b>Ctrl + Alt + Delete</b> and use the Windows secure screen if necessary.",
                ],
                body,
            ),
            Paragraph("The desktop cat is missing", h2),
            bullet_list(
                [
                    "Confirm Desktop cat is not set to No cat.",
                    "Look across all connected monitors; CatX uses the complete Windows virtual desktop.",
                    "Disable and re-enable the guard after changing the cat selection.",
                ],
                body,
            ),
            Paragraph("The recovery shortcut does not respond", h2),
            bullet_list(
                [
                    "Hold all keys in the selected combination at the same time.",
                    "Try the left and right Ctrl, Alt, or Shift keys.",
                    "Use the mouse fallback and verify the selected shortcut before enabling the guard again.",
                ],
                body,
            ),
            Paragraph("CatX does not start", h2),
            bullet_list(
                [
                    "Confirm the PC runs 64-bit Windows 11.",
                    "Reinstall CatX from the official release.",
                    "If security software quarantined CatX, verify the installer source before restoring or allowing it.",
                ],
                body,
            ),
            PageBreak(),
        ]
    )

    story.extend(
        [
            Paragraph("Privacy, uninstall, and support", h1),
            Paragraph("Privacy", h2),
            Paragraph(
                "CatX does not collect typed content, analytics, account information, or personal data. It makes no network requests. Preferences are stored locally in:",
                body,
            ),
            Paragraph("%LOCALAPPDATA%\\CatX\\settings.json", code),
            Paragraph(
                "Uninstalling CatX removes the application. The settings file may remain so preferences can be restored after reinstalling; it can be deleted manually while CatX is closed.",
                body,
            ),
            Paragraph("Uninstall", h2),
            numbered_list(
                [
                    "Open Windows <b>Settings</b>.",
                    "Select <b>Apps</b>, then <b>Installed apps</b>.",
                    "Find <b>CatX Keyboard Guard</b>.",
                    "Choose <b>Uninstall</b> and follow the prompts.",
                ],
                body,
            ),
            Paragraph("Support", h2),
            Paragraph(
                "Project and release information: <link href='https://github.com/digitalrcs/CatX' color='#008F95'>github.com/digitalrcs/CatX</link>",
                body,
            ),
            Paragraph(
                "When reporting a reproducible problem, include the CatX version, Windows version, CPU architecture, keyboard layout, selected recovery shortcut, and steps to reproduce. Never share passwords, authentication codes, private typed content, or security tokens.",
                body,
            ),
            Spacer(1, 0.2 * inch),
            callout(
                "CatX is original software from <b>Dan Roberts - DigitalRCS</b>.",
                centered,
                CREAM,
                CORAL,
            ),
        ]
    )

    document.build(story, onFirstPage=first_page, onLaterPages=later_page)
    print(OUTPUT)


if __name__ == "__main__":
    build_pdf()
