require "BaseParser"

Class.CSharpParser = {
	__inherit = "BaseParser",
}

function CSharpParser:ParseFile(file)
	local lines = self:GetFileLines(file)
	local nlines = #lines
	local nodes = { }

	for i=1,nlines do
		lines[i] = string.trim_spaces(lines[i])
	end

	local i = 1
	while true do
		if i > nlines then break end
		local line = lines[i]

		if string.starts_with(line, "///") then
			local block = { line }
			--read whole comment block
			while true do
				if i > nlines then break end
				local line = lines[i]

				if string.starts_with(line, "///") then
					table.insert(block, line)
				else
					break
				end

				i = i + 1
			end

			table.insert(nodes, self:ParseBlock(block))
		end

		i = i + 1
	end

	return nodes
end

function CSharpParser:ParseBlock(block)
	local nblock = #block

	--remove the "/// "
	for i=1,nblock do
		block[i] = string.sub(block[i], 4)
	end

	local node

	local tag, attributes, content, i = self:NextTag(block, 1)
	while tag do
		print("found tag", tag, "saying", "\""..tostring(content).."\"")
		if node then
			if tag == "summary" then
				node.summary = content
			end
		else
			if tag == "class" then
				node = ClassNode:new()
				node.title = content
			elseif tag == "method" then
				node = MethodNode:new()
				node.title = content
			elseif tag == "property" then
				node = PropertyNode:new()
				node.title = content
			end
		end

		tag, attributes, content, i = self:NextTag(block, i)
	end

	return node
end

function CSharpParser:NextTag(block, first)
	local nblock = #block
	local tag, attributes, content, last

	local i = first
	while true do
		if i > nblock then break end
		local line = block[i]

		--opening tag
		if string.find(line, "<[%w%d_]+>") then
			tag = string.gfind(line, "<([%w%d_]+)>")
			attributes = { }
			content = { }
			local closing_tag_pattern = "</" .. tag .. ">"
			while true do
				if i > nblock then break end
				local line = block[i]
				if string.find(line, closing_tag_pattern) then break end
				table.insert(content, line)
				i = i + 1
			end

			content = table.concat(content, "\n")
			last = i + 1
			break
		--one line
		elseif string.find(line, "<.+%s*/%s*>") then
			tag = string.gfind(line, "<([%w%d_]+)>")
			attributes = { }
			content = ""
			last = i + 1
			break
		end

		i = i + 1
	end

	return tag, attributes, content, last
end
