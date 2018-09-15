require "DocWriter"

Class.MarkdeepWriter = {
	__inherit = "DocWriter",
}

function MarkdeepWriter:GenerateFileFooter(node)
	return "<!-- Markdeep: --><style class=\"fallback\">body{visibility:hidden;white-space:pre;font-family:monospace}</style>\n<script src=\"markdeep.min.js\"></script>\n<script src=\"https://casual-effects.com/markdeep/latest/markdeep.min.js\"></script>\n<script>window.alreadyProcessedMarkdeep||(document.body.style.visibility=\"visible\")</script>"
end

function MarkdeepWriter:GenerateList(list)
	local result = ""
	for i,line in ipairs(list) do
		result = result .. "- " .. line .. "\n"
	end

	return result
end

function MarkdeepWriter:GenerateCaption(caption)
	return string.format("**%s**\n", tostring(caption))
end

function MarkdeepWriter:GenerateSection(section)
	return string.format("\n%s\n%s\n", section, string.rep("=", #section))
end

function MarkdeepWriter:GenerateSnippet(code, language, description)
	local result = "~~~"
	if language then
		result = result .. " " .. language
	end

	result = result .. "\n"
	result = result .. code .. "\n"
	result = result .. "~~~"

	if description then
		result = result .. "\n" .. description
	end

	return result
end

function MarkdeepWriter:GenerateTable(t, columns)
	local _t
	if columns then
		_t = { columns }
		for i=1,#t do
			_t[i + 1] = t[i]
		end
	else
		columns = t[1]
		_t = t
	end

	local column_widths = { }
	for col=1,#columns do
		column_widths[col] = 0
		for row=1,#_t do
			local len = #_t[row][col]
			if column_widths[col] < len then
				column_widths[col] = len
			end
		end
	end

	local result = ""

	for row=1,#_t do
		local line = ""
		for col=1,#columns do
			local cell = _t[row][col]
			local len = #cell
			line = line .. cell .. string.rep(" ", column_widths[col] - len) .. " |"
		end
		if row == 1 then
			line = line .. "\n"
			for col=1,#columns do
				line = line .. string.rep("-", column_widths[col]) .. "-|"
			end
		end
		result = result .. line .. "\n"
	end

	return result
end

function MarkdeepWriter:GenerateLink(url, text)
	return string.format("[%s](%s)", text, url)
end

function MarkdeepWriter:GenerateOrderedListLine(text)
	local line = DocWriter.GenerateOrderedListLine(self, text)
	return string.format("\n%d. %s\n", self.list_num, line)
end
